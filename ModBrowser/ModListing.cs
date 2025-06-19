using SporeCommunity.ModBrowser.ModIdentity;
using System;
using System.Xml.Linq;

namespace SporeCommunity.ModBrowser
{
    public class ModListing
    {
        /// <summary>
        /// The Mod Identity for this mod. Contains most metadata about the mod.
        /// </summary>
        public XmlModIdentity ModIdentity { get; }

        /// <summary>
        /// The name of the mod, as shown to the user.
        /// </summary>
        public string DisplayName => ModIdentity.DisplayName;

        /// <summary>
        /// The version number of this mod.
        /// </summary>
        public string? Version { get; }

        /// <summary>
        /// A short description of this mod's features.
        /// </summary>
        public string? Description { get; }

        /// <summary>
        /// The developer of this mod.
        /// </summary>
        public string Author { get; }

        /// <summary>
        /// Whether the mod is experimental (work-in-progress/pre-release). If true, users should be warned before installing this mod.
        /// </summary>
        public bool IsExperimental => ModIdentity.IsExperimental;

        /// <summary>
        /// Whether this mod requires a galaxy reset (save data reset) to take effect. If true, users should be warned before installing this mod.
        /// </summary>
        public bool RequiresGalaxyReset => ModIdentity.RequiresGalaxyReset;

        /// <summary>
        /// Whether saved games played with this mod will be unplayable or damaged, if this mod is uninstalled. If true, users should be warned before installing this mod.
        /// </summary>
        public bool CausesSaveDataDependency => ModIdentity.CausesSaveDataDependency;

        /// <summary>
        /// The Git repository for this mod.
        /// </summary>
        public Uri? RepositoryUrl { get; }

        /// <summary>
        /// The project website for this mod.
        /// </summary>
        public Uri? ProjectUrl { get; }

        /// <summary>
        /// The URL of the .sporemod file download.
        /// </summary>
        public Uri? DownloadUrl { get; }

        /// <summary>
        /// Gets the date and time that this version of the mod was uploaded.
        /// </summary>
        public DateTime? LastUpdatedDate { get; }

        /// <summary>
        /// The number of times this version of the mod has been downloaded.
        /// </summary>
        public int? DownloadCount { get; }

        public ModListing(XElement xmlModIdentity, string? version, string? description, string author, Uri? repositoryUrl, Uri? projectUrl, Uri? downloadUrl, DateTime? lastUpdated, int? downloadCount)
        {
            ModIdentity = new XmlModIdentity(xmlModIdentity);

            // Prefer version and description from Mod Identity, if present
            Version = ModIdentity.ModVersion?.ToString() ?? version;
            Description = ModIdentity.Description ?? description;

            Author = author;

            RepositoryUrl = repositoryUrl;
            ProjectUrl = projectUrl;

            // Make sure download URL is valid (ends with .sporemod or .package)
            if (downloadUrl is not null && (downloadUrl.AbsoluteUri.EndsWith(".sporemod") || downloadUrl.AbsoluteUri.EndsWith(".package")))
            {
                DownloadUrl = downloadUrl;
            }

            LastUpdatedDate = lastUpdated;
            DownloadCount = downloadCount;
        }
    }
}
