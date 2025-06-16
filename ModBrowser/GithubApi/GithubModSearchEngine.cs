using SporeCommunity.ModBrowser.ModIdentity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SporeCommunity.ModBrowser.GithubApi
{
    public class GithubModSearchEngine : IModSearchEngine
    {
        private readonly GithubApiClient client;

        /// <summary>
        /// Creates a new GitHub client for searching for Spore mods.
        /// </summary>
        /// <param name="userAgent">A User-Agent header. GitHub requires that you provide your GitHub username, or name of the application, so they can contact you if there are problems.</param>
        public GithubModSearchEngine(string userAgent)
        {
            client = new(userAgent);
        }

        public async IAsyncEnumerable<ModListing> SearchModsAsync(string searchTerm = "")
        {
            var repos = await SearchForSporeModRepositoriesAsync(searchTerm);

            foreach (var repo in repos)
            {
                var modListing = await GetModListingFromRepositoryAsync(repo);
                if (modListing is not null)
                    yield return modListing;
            }
        }

        /// <summary>
        /// Searches GitHub for Spore mods that match the specified search term.
        /// </summary>
        /// <param name="searchTerm">The term to search for. All GitHub search modifiers are allowed.</param>
        /// <returns>A list of repositories containing Spore mods.</returns>
        private async Task<IEnumerable<Repository>> SearchForSporeModRepositoriesAsync(string searchTerm = "")
        {
            // Restrict searches to repos that have the spore-mod topic
            if (searchTerm.Length > 0) searchTerm += " ";
            searchTerm += "topic:spore-mod";

            var repos = await client.SearchForRepositoriesAsync(searchTerm);
            return repos;
        }

        /// <summary>
        /// Gets the Spore mod listing for the specified GitHub repository.
        /// The ModInfo.xml file in the repository will be used to generate the mod listing.
        /// </summary>
        /// <param name="repository">The GitHub repository containing the Spore mod.</param>
        /// <returns>The Mod Listing for the Spore mod, or null if the repository does not contain a valid ModInfo.xml file.</returns>
        private async Task<ModListing?> GetModListingFromRepositoryAsync(Repository repository)
        {
            var modIdentity = await repository.GetModIdentityAsync();

            // Get download info and URL
            var asset = await repository.GetLatestAssetAsync();

            // Get download version
            var versionString = asset?.Version;
            Version? version = null;
            if(versionString is not null)
            {
                versionString = versionString.Replace("v", "").Trim();
                Version.TryParse(versionString, out version);
            }

            if (modIdentity is null)
            {
                // Don't return a mod listing if Mod Identity is missing
                return null;
            }

            try
            {
                var listing = new ModListing(modIdentity, version, repository.Description, repository.Owner, repository.RepositoryUrl, repository.ProjectUrl, asset?.DownloadUrl, asset?.PublishedAt, asset?.DownloadCount);
                return listing;
            }
            catch (ModAttributeException)
            {
                // Don't return a mod listing if Mod Identity is invalid
                return null;
            }
        }
    }
}
