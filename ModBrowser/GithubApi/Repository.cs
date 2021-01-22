using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SporeCommunity.ModBrowser.GithubApi
{
    /// <summary>
    /// Represents a GitHub repository.
    /// </summary>
    public class Repository
    {
        /// <summary>
        /// The GitHub API client, used for retrieving files in this repository.
        /// </summary>
        private readonly GithubApiClient client;

        /// <summary>
        /// Creates an instance for a GitHub repository from JSON data returned by the GitHub API.
        /// </summary>
        /// <param name="client">The GitHub API client.</param>
        /// <param name="jsonData">The JSON data containing the information for this repository.</param>
        internal Repository(GithubApiClient client, JToken jsonData)
        {
            this.client = client;

            // As long as the GitHub API is working as expected, these values cannot be null
            // But the compiler doesn't know that, so we need !
            Name = (string)jsonData["name"]!;
            Owner = (string)jsonData["owner"]!["login"]!;
            RepositoryUrl = new Uri((string)jsonData["html_url"]!);
            Description = (string?)jsonData["description"];
            Created = DateTime.ParseExact((string)jsonData["created_at"]!, "MM/dd/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            Updated = DateTime.ParseExact((string)jsonData["updated_at"]!, "MM/dd/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            ProjectUrl = Uri.TryCreate((string?)jsonData["homepage"], UriKind.Absolute, out var homepage) ? homepage : null;
        }

        public string Name { get; init; }
        public string Owner { get; init; }
        public Uri RepositoryUrl { get; init; }
        public string? Description { get; init; }
        public DateTime Created { get; init; }
        public DateTime Updated { get; init; }
        public Uri? ProjectUrl { get; init; }

        private async Task<string> GetFileAsync(string path) => await client.GetFileAsync(Owner, Name, path);

        public async Task<XElement> GetModIdentityAsync() => XElement.Parse(await GetFileAsync("ModInfo.xml"));

        public async Task<string> GetReadmeAsync() => await GetFileAsync("README.md");

        public async Task<Uri?> GetDownloadUrlAsync()
        {
            var endpoint = $"repos/{Owner}/{Name}/releases/latest";
            try
            {
                var json = await client.GetJsonAsync(endpoint);
                var url = (string?)json["assets"]?[0]?["browser_download_url"];
                return Uri.TryCreate(url, UriKind.Absolute, out var result) ? result : null;
            }
            catch
            {
                // Runs when no releases were found
                return null;
            }
        }
    }
}
