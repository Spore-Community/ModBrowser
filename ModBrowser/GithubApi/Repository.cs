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
            Description = (string)jsonData["description"]!;
            Created = DateTime.ParseExact((string)jsonData["created_at"]!, "MM/dd/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            Updated = DateTime.ParseExact((string)jsonData["updated_at"]!, "MM/dd/yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            ProjectUrl = new Uri((string)jsonData["homepage"]!);
        }

        internal Repository(GithubApiClient client) => this.client = client;

        public string Name { get; init; }
        public string Owner { get; init; }
        public Uri RepositoryUrl { get; init; }
        public string? Description { get; init; }
        public DateTime Created { get; init; }
        public DateTime Updated { get; init; }
        public Uri? ProjectUrl { get; init; }

        private async Task<string> GetFile(string path) => await client.GetFileAsync(Owner, Name, path);

        public async Task<XElement> GetModIdentity() => XElement.Parse(await GetFile("ModInfo.xml"));

        public async Task<string> GetReadme() => await GetFile("README.md");
    }
}
