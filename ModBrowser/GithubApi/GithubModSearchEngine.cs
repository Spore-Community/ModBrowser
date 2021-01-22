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

        public async Task<List<ModListing>> SearchModsAsync(string searchTerm = "")
        {
            var repos = await SearchForSporeModRepositoriesAsync(searchTerm);

            var mods = new List<ModListing>();

            foreach (var repo in repos)
            {
                var modListing = await GetModListingFromRepositoryAsync(repo);
                mods.Add(modListing);
            }

            return mods;
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
        /// <returns>The Mod Listing for the Spore mod.</returns>
        private async Task<ModListing> GetModListingFromRepositoryAsync(Repository repository)
        {
            var modIdentity = await repository.GetModIdentityAsync();

            // TODO: Handle what happens when Mod Identity is not present or invalid
            //       - Exception for missing/invalid Mod Identity
            //       - Fill in description from GitHub is one is not present in Mod Identity
            //       - Future versions of Mod Identity may include repo URL

            var listing = new ModListing(modIdentity, repository.Owner, repository.RepositoryUrl, repository.ProjectUrl, await repository.GetDownloadUrlAsync());
            return listing;
        }
    }
}
