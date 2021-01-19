﻿using Newtonsoft.Json.Linq;
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
        private readonly HttpClient HttpClient = new HttpClient() { BaseAddress = new Uri("https://api.github.com/") };

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
        /// <param name="query">The query string, does not need to be encoded.</param>
        /// <returns>A JSON object representing the result.</returns>
        private async Task<JObject> GetJsonAsync(string endpoint, string? query = null)
        {
            // Build uri
            string uri = endpoint;
            if (query != null)
            {
                uri += "?" + query;//WebUtility.UrlEncode(query);
            }

            // Execute request
            var result = await HttpClient.GetAsync(uri);

            // Make sure status code was 200 OK
            if (!result.IsSuccessStatusCode)
            {
                /*// Check if rate limit exceeded
                if (result.Headers.GetValues("x-ratelimit-remaining").FirstOrDefault() == "0")
                {
                    var rateLimit = result.Headers.GetValues("x-ratelimit-limit").First();
                    var rateLimitReset = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(double.Parse(result.Headers.GetValues("x-ratelimit-reset").First())).ToLocalTime();

                    var rateLimitResetDelta = DateTime.Now.Subtract(rateLimitReset);

                    throw new Exception($"GitHub rate limit exceeded. The current limit is {rateLimit} and it resets at {rateLimitReset}. Try again in {-Math.Ceiling(rateLimitResetDelta.TotalSeconds)} seconds.");
                }
                throw new Exception($"GitHub API error {result.StatusCode}: {result.ReasonPhrase} - Response: {await result.Content.ReadAsStringAsync()} - Headers: {result.Headers}");*/
                throw new GithubApiException(result);
            }

            // Return as JSON object
            var content = await result.Content.ReadAsStringAsync();
            return JObject.Parse(content);
        }

        public async Task<IEnumerable<Repository>> SearchForRepositoriesAsync(string searchTerm = "")
        {
            var endpoint = "search/repositories";
            var query = "q=" + searchTerm;
            var json = await GetJsonAsync(endpoint, query);

            // If above line didn't raise an exception, the contents will be present, so this cannot be null
            var repos = (JArray)json["items"]!;

            var repoQuery = from repo in repos
                            select new Repository(this)
                            {
                                // As long as the GitHub API is working as expected, these values cannot be null
                                // But the compiler doesn't know that, so we need !
                                Name = (string)repo["name"]!,
                                Owner = (string)repo["owner"]!["login"]!,
                                RepositoryUrl = new Uri((string)repo["html_url"]!),
                                Description = (string)repo["description"]!,
                                Created = DateTime.ParseExact((string)repo["created_at"]!, "MM/dd/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
                                Updated = DateTime.ParseExact((string)repo["updated_at"]!, "MM/dd/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
                                ProjectUrl = new Uri((string)repo["homepage"]!)
                            };
            return repoQuery;
        }

        /*public async Task<IEnumerable<Repository>> SearchForSporeModRepositoriesAsync(string searchTerm = "")
        {
            // Restrict searches to repos that have the spore-mod topic
            if (searchTerm.Length > 0) searchTerm += " ";
            searchTerm += "topic:spore-mod";

            var repos = await SearchForRepositoriesJsonAsync(searchTerm);

            var repoQuery = from repo in repos
                            select new Repository(this)
                            {
                                // As long as the GitHub API is working as expected, these values cannot be null
                                // But the compiler doesn't know that, so we need !
                                Name = (string)repo["name"]!,
                                Owner = (string)repo["owner"]!["login"]!,
                                RepositoryUrl = new Uri((string)repo["html_url"]!),
                                Description = (string)repo["description"]!,
                                Created = DateTime.ParseExact((string)repo["created_at"]!, "MM/dd/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
                                Updated = DateTime.ParseExact((string)repo["updated_at"]!, "MM/dd/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
                                ProjectUrl = new Uri((string)repo["homepage"]!)
                            };
            return repoQuery;
        }*/

        /// <summary>
        /// Gets a raw file from the specified endpoint.
        /// </summary>
        /// <param name="endpoint">The endpoint to access.</param>
        /// <returns>A string containing the raw contents of the file.</returns>
        public async Task<string> GetFileAsync(string endpoint)
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

            return content;
        }

        /// <summary>
        /// Gets a file contained in a repository. The file must be in the repository's default branch.
        /// </summary>
        /// <param name="repositoryOwner">The name of the repository's owner (username or organization name).</param>
        /// <param name="repositoryName">The name of the repository.</param>
        /// <param name="path">The file path, with no leading slash.</param>
        /// <returns>A string containing the raw contents of the file.</returns>
        public async Task<string> GetFileAsync(string repositoryOwner, string repositoryName, string path)
        {
            var endpoint = $"repos/{repositoryOwner}/{repositoryName}/contents/{path}";
            return await GetFileAsync(endpoint);
        }
    }
}