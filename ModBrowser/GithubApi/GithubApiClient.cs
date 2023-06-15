using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace SporeCommunity.ModBrowser.GithubApi
{
    /// <summary>
    /// Provides access to the GitHub web API.
    /// </summary>
    public class GithubApiClient
    {
        /// <summary>
        /// The HTTP client used to communicate witth the GitHub web API.
        /// </summary>
        private readonly HttpClient HttpClient = new HttpClient() { BaseAddress = new Uri("https://sporemodbrowser-gh-api.kade.workers.dev/") };

        /// <summary>
        /// Creates a new HTTP client for accessing the GitHub web API.
        /// </summary>
        /// <param name="userAgent">A User-Agent header. GitHub requires that you provide your GitHub username, or name of the application, so they can contact you if there are problems.</param>
        public GithubApiClient(string userAgent)
        {
            // Media types - API version
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.mercy-preview+json")); // required for topic search

            // TODO authentication with client ID and secret, to increase rate limits (no github login needed)
            //HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "clientId:clientSecret");

            // User agent - GitHub will track this
            HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
        }

        /// <summary>
        /// Executes a GET request against the specified endpoint, with the specified query string.
        /// </summary>
        /// <param name="endpoint">The endpoint to retrieve data from, with no leading slash.</param>
        /// <param name="query">The query string.</param>
        /// <returns>A JSON object representing the result.</returns>
        public async Task<JObject> GetJsonAsync(string endpoint, string? query = null)
        {
            // Build uri
            string uri = endpoint;
            if (query != null)
            {
                uri += "?" + query;
            }

            // Execute request
            var result = await HttpClient.GetAsync(uri);

            // Make sure status code was 200 OK
            if (!result.IsSuccessStatusCode)
            {
                throw new GithubApiException(result);
            }

            // Return as JSON object
            var content = await result.Content.ReadAsStringAsync();
            return JObject.Parse(content);
        }

        /// <summary>
        /// Searches GitHub for repositories that match the specified search term.
        /// </summary>
        /// <param name="searchTerm">The term to search for. All GitHub search modifiers are allowed.</param>
        /// <returns>A list of repositories matching the search term.</returns>
        public async Task<IEnumerable<Repository>> SearchForRepositoriesAsync(string searchTerm = "")
        {
            var endpoint = "search/repositories";
            var query = "q=" + searchTerm;
            var json = await GetJsonAsync(endpoint, query);

            // If above line didn't raise an exception, the contents will be present, so this cannot be null
            var repos = (JArray)json["items"]!;

            var repoQuery = from repo in repos
                            select new Repository(this, repo);
            return repoQuery;
        }

        /// <summary>
        /// Gets a repository by its name.
        /// </summary>
        /// <param name="repositoryOwner">The name of the repository's owner (username or organization name).</param>
        /// <param name="repositoryName">The name of the repository.</param>
        /// <returns>The repository.</returns>
        public async Task<Repository> GetRepositoryAsync(string repositoryOwner, string repositoryName)
        {
            var endpoint = $"repos/{repositoryOwner}/{repositoryName}";
            var json = await GetJsonAsync(endpoint);

            var repo = new Repository(this, json);
            return repo;
        }

        /// <summary>
        /// Gets a raw file from the specified endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint to access.</param>
        /// <returns>A string containing the raw contents of the file, or null if the file did not exist.</returns>
        public async Task<string?> GetFileAsync(string endpoint)
        {
            // Construct the request
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(HttpClient.BaseAddress, endpoint),
                Method = HttpMethod.Get
            };
            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3.raw"));

            // Execute request and get resulting content
            var result = await HttpClient.SendAsync(request);
            var content = await result.Content.ReadAsStringAsync();

            // Make sure status code was 200 OK
            if (!result.IsSuccessStatusCode)
            {
                if(result.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null;
                }
                throw new GithubApiException(result);
            }

            return content;
        }

        /// <summary>
        /// Gets a file contained in a repository. The file must be in the repository's default branch.
        /// </summary>
        /// <param name="repositoryOwner">The name of the repository's owner (username or organization name).</param>
        /// <param name="repositoryName">The name of the repository.</param>
        /// <param name="path">The file path, with no leading slash.</param>
        /// <returns>A string containing the raw contents of the file, or null if the file did not exist.</returns>
        public async Task<string?> GetFileAsync(string repositoryOwner, string repositoryName, string path)
        {
            var endpoint = $"repos/{repositoryOwner}/{repositoryName}/contents/{path}";
            return await GetFileAsync(endpoint);
        }
    }
}
