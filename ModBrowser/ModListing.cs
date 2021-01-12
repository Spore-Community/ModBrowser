using System;
using System.Xml.Linq;

namespace SporeCommunity.ModBrowser
{
    public class ModListing
    {
        /// <summary>
        /// The name of the mod, as shown to the user.
        /// </summary>
        public string DisplayName { get; init; }

        /// <summary>
        /// The unique name of the mod, used internally.
        /// </summary>
        public string UniqueName { get; init; }

        /// <summary>
        /// The version number of this mod.
        /// </summary>
        public Version? Version { get; init; }

        /// <summary>
        /// A short description of this mod's features.
        /// </summary>
        public string? Description { get; init; }

        /// <summary>
        /// The developer of this mod.
        /// </summary>
        public string Author { get; init; }

        /// <summary>
        /// The version of this mod's XML Mod Identity.
        /// </summary>
        public Version InstallerSystemVersion { get; init; }

        /// <summary>
        /// The minimum version of the Spore ModAPI that is required for this mod.
        /// </summary>
        public Version DllsBuild { get; init; }

        /// <summary>
        /// Whether the mod is experimental (work-in-progress/pre-release). If true, users should be warned before installing this mod.
        /// </summary>
        public bool IsExperimental { get; init; }

        /// <summary>
        /// Whether this mod requires a galaxy reset (save data reset) to take effect. If true, users should be warned before installing this mod.
        /// </summary>
        public bool RequiresGalaxyReset { get; init; }

        /// <summary>
        /// Whether saved games played with this mod will be unplayable or damaged, if this mod is uninstalled. If true, users should be warned before installing this mod.
        /// </summary>
        public bool CausesSaveDataDependency { get; init; }

        /// <summary>
        /// The Git repository for this mod.
        /// </summary>
        public Uri RepositoryUrl { get; init; }

        /// <summary>
        /// The project website for this mod.
        /// </summary>
        public Uri? ProjectUrl { get; init; }

        /// <summary>
        /// The URL of the .sporemod file download.
        /// </summary>
        public Uri? DownloadUrl { get; init; }

        public ModListing(XElement xmlModIdentity, string author, Uri repositoryUrl, Uri? projectUrl, Uri? downloadUrl)
        {
            InstallerSystemVersion = Version.Parse(xmlModIdentity.Attribute("installerSystemVersion").Value);

            // If other Mod Identity XML versions are introduced, need to check that here

            // TODO: Rewrite all of this because it is very broken
            //       - Need to handle cases where some (or all) of these values are missing

            // Possibly move Mod Identity out to its own class entirely

            DisplayName = xmlModIdentity.Attribute("displayName").Value;
            UniqueName = xmlModIdentity.Attribute("unique").Value;
            Description = xmlModIdentity.Attribute("description").Value;

            Version = Version.Parse(xmlModIdentity.Attribute("modVersion").Value);
            DllsBuild = Version.Parse(xmlModIdentity.Attribute("dllsBuild").Value);

            IsExperimental = (bool)xmlModIdentity.Attribute("isExperimental");
            RequiresGalaxyReset = (bool)xmlModIdentity.Attribute("requiresGalaxyReset");
            CausesSaveDataDependency = (bool)xmlModIdentity.Attribute("causesSaveDataDependency");

            Author = author;

            RepositoryUrl = repositoryUrl;
            ProjectUrl = projectUrl;
            DownloadUrl = downloadUrl;
        }
    }
}
