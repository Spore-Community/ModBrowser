using System;
using System.Globalization;
using System.Text.Json;
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
        ///   Attempts to retrieve mod file asset
        /// </summary>
        private JsonElement? GetSporeModAsset(JsonElement assets)
        {
            for (int i = 0; i < assets.GetArrayLength(); i++)
            {
                if (assets[i].TryGetProperty("name", out JsonElement nameElement))
                {
                    string? name = nameElement.GetString();
                    string lowerCaseName = name == null ? "" : name.ToLowerInvariant();

                    if (lowerCaseName.EndsWith(".package") ||
                        lowerCaseName.EndsWith(".sporemod"))
                    {
                        return assets[i];
                    }
                }
            }

            return null;
        }

        /// <summary>
        ///     Returns whether release is not a draft or prerelease
        /// </summary>
        private bool IsValidRelease(JsonElement release)
        {
            if (release.TryGetProperty("draft", out JsonElement draftElement) &&
                release.TryGetProperty("prerelease", out JsonElement prereleaseElement))
            {
                return !draftElement.GetBoolean() && !prereleaseElement.GetBoolean();
            }

            return false;
        }

        /// <summary>
        ///     Returns latest valid release
        /// </summary>
        private JsonElement GetLatestRelease(JsonElement releases)
        {
            for (int i = 0; i < releases.GetArrayLength(); i++)
            {
                if (IsValidRelease(releases[i]))
                {
                    return releases[i];
                }
            }

            throw new Exception("no valid releases found!");
        }

        /// <summary>
        ///   Retrieves sum of all valid asset download count
        /// </summary>
        private int? GetDownloadCount(JsonElement releases)
        {
            bool hasCount = false;
            int sum = 0;

            for (int i = 0; i < releases.GetArrayLength(); i++)
            {
                if (IsValidRelease(releases[i]) &&
                    releases[i].TryGetProperty("assets", out JsonElement assets))
                {
                    JsonElement? asset = GetSporeModAsset(assets);
                    if (asset is JsonElement assetElement &&
                        assetElement.TryGetProperty("download_count", out JsonElement downloadCountElement) &&
                        downloadCountElement.TryGetInt32(out int assetDownloadCount))
                    {
                        sum += assetDownloadCount;
                        hasCount = true;
                    }
                }
            }

            return hasCount ? sum : null;
        }

        /// <summary>
        /// Creates an instance for a GitHub repository from JSON data returned by the GitHub API.
        /// </summary>
        /// <param name="client">The GitHub API client.</param>
        /// <param name="jsonData">The JSON data containing the information for this repository.</param>
        internal Repository(GithubApiClient client, JsonElement jsonData)
        {
            this.client = client;

            // As long as the GitHub API is working as expected, these values cannot be null
            // But the compiler doesn't know that, so we need !
            Name = jsonData.GetProperty("name").GetString()!;
            Owner = jsonData.GetProperty("owner").GetProperty("login").GetString()!;
            RepositoryUrl = new Uri(jsonData.GetProperty("html_url").GetString()!);
            Description = jsonData.TryGetProperty("description", out var description) ? description.GetString() : null;
            Created = DateTime.Parse(jsonData.GetProperty("created_at").GetString()!, null, DateTimeStyles.AdjustToUniversal).ToLocalTime();
            Updated = DateTime.Parse(jsonData.GetProperty("updated_at").GetString()!, null, DateTimeStyles.AdjustToUniversal).ToLocalTime();
            ProjectUrl = Uri.TryCreate(jsonData.TryGetProperty("homepage", out var homepageString) ? homepageString.GetString() : null, UriKind.Absolute, out var homepage) ? homepage : null;
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
            var endpoint = $"repos/{Owner}/{Name}/releases";
            try
            {
                using var json = await client.GetJsonAsync(endpoint);
                var releaseJson = GetLatestRelease(json.RootElement);

                JsonElement assets;
                releaseJson.TryGetProperty("assets", out assets);

                JsonElement? asset = GetSporeModAsset(assets);

                var url = asset?.GetProperty("browser_download_url").GetString();
                var uri = Uri.TryCreate(url, UriKind.Absolute, out var result) ? result : null;

                var version = releaseJson.GetProperty("tag_name").GetString()!;

                var publishedAt = DateTime.Parse(releaseJson.GetProperty("published_at").GetString()!, null, DateTimeStyles.AdjustToUniversal).ToLocalTime();

                int? downloadCount = GetDownloadCount(json.RootElement);

                return new Asset()
                {
                    DownloadUrl = uri,
                    Version = version,
                    PublishedAt = publishedAt,
                    DownloadCount = downloadCount
                };
            }
            catch (Exception)
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
