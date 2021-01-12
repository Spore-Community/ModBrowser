using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SporeCommunity.ModBrowser
{
    interface IModSearchEngine
    {
        /// <summary>
        /// Searches for Spore mods that match the given search term.
        /// </summary>
        /// <param name="searchTerm">The term to search for.</param>
        /// <returns>A list of Spore mods tthat match the given search term.</returns>
        public Task<List<ModListing>> SearchModsAsync(string searchTerm = "");
    }
}
