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

        public string Name { get; }
        public string Owner { get; }
        public Uri RepositoryUrl { get; }
        public string? Description { get; }
        public DateTime Created { get; }
        public DateTime Updated { get; }
        public Uri? ProjectUrl { get; }

        /// <summary>
        /// Gets a file in this repository's default branch.
        /// </summary>
        /// <param name="path">The file path, with no leading slash.</param>
        /// <returns>A string containing the raw contents of the file, or null if the file did not exist.</returns>
        private async Task<string?> GetFileAsync(string path) => await client.GetFileAsync(Owner, Name, path);

        /// <summary>
        /// Gets the XML Mod Identity (ModInfo.xml) file in this repository.
        /// </summary>
        /// <returns>The XML file, or null if it was missing or malformed.</returns>
        public async Task<XElement?> GetModIdentityAsync()
        {
            var file = await GetFileAsync("ModInfo.xml");

            if (file is null) return null;

            try
            {
                return XElement.Parse(file);
            }
            catch (Exception)
            {
                // If XML doc was invalid, return null
                return null;
            }
        }

        /// <summary>
        /// Gets the readme.md file in this repository.
        /// </summary>
        /// <returns>The readme file, or null if it was missing.</returns>
        public async Task<string?> GetReadmeAsync() => await GetFileAsync("README.md");

        /// <summary>
        /// Gets the latest released asset from GitHub Releases.
        /// </summary>
        /// <returns>The asset, or null if there are no releases.</returns>
        public async Task<Asset?> GetLatestAssetAsync()
        {
            var endpoint = $"repos/{Owner}/{Name}/releases/latest";
            try
            {
                var json = await client.GetJsonAsync(endpoint);

                var url = (string?)json["assets"]?[0]?["browser_download_url"];
                var uri = Uri.TryCreate(url, UriKind.Absolute, out var result) ? result : null;

                var version = (string)json["tag_name"]!;

                var publishedAt = DateTime.ParseExact((string)json["published_at"]!, "MM/dd/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                var downloadCount = (int?)json["assets"]?[0]?["download_count"];

                return new Asset()
                {
                    DownloadUrl = uri,
                    Version = version,
                    PublishedAt = publishedAt,
                    DownloadCount = downloadCount
                };
            }
            catch (GithubApiException)
            {
                // Runs when no releases were found
                return null;
            }
        }

        public struct Asset
        {
            public Uri? DownloadUrl { get; init; }
            public string Version { get; init; }
            public DateTime PublishedAt { get; init; }
            public int? DownloadCount { get; init; }
        }
    }
}
