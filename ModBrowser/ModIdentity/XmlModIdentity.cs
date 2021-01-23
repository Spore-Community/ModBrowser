using System;
using System.Xml.Linq;

namespace SporeCommunity.ModBrowser.ModIdentity
{
    public class XmlModIdentity
    {
        /// <summary>
        /// The unique name of the mod, used internally.
        /// </summary>
        public string Unique { get; }

        /// <summary>
        /// The name of the mod, as shown to the user.
        /// </summary>
        public string DisplayName { get; }

        /// <summary>
        /// A short description of this mod's features.
        /// </summary>
        public string? Description { get; }

        /// <summary>
        /// The version number of this mod.
        /// </summary>
        public Version? ModVersion { get; }

        /// <summary>
        /// The version of this mod's XML Mod Identity.
        /// </summary>
        public Version InstallerSystemVersion { get; }

        /// <summary>
        /// The minimum version of the Spore ModAPI that is required for this mod.
        /// </summary>
        public Version DllsBuild { get; }

        /// <summary>
        /// Whether this mod can be disabled by the user.
        /// </summary>
        public bool CanDisableMod { get; }

        /// <summary>
        /// Whether this mod features a custom installer, allowing the user to choose which components to enable.
        /// </summary>
        public bool HasCustomInstaller { get; }

        /// <summary>
        /// Whether the mod is experimental (work-in-progress/pre-release). If true, users should be warned before installing this mod.
        /// </summary>
        public bool IsExperimental { get; }

        /// <summary>
        /// Whether this mod requires a galaxy reset (save data reset) to take effect. If true, users should be warned before installing this mod.
        /// </summary>
        public bool RequiresGalaxyReset { get; }

        /// <summary>
        /// Whether saved games played with this mod will be unplayable or damaged, if this mod is uninstalled. If true, users should be warned before installing this mod.
        /// </summary>
        public bool CausesSaveDataDependency { get; }

        /// <summary>
        /// Creates an XML Mod Identity from the given XML document.
        /// </summary>
        /// <param name="xmlDocument">The &lt;Mod&gt; XML element.</param>
        public XmlModIdentity(XElement xmlDocument)
        {
            // Convenience function for generating exception messages
            string GetModIdentityString() => "Mod Identity" + (Unique is not null ? " for " + Unique : "");

            // Parse a required string from XML doc
            string ParseString(string attributeName)
            {
                return xmlDocument.Attribute(attributeName)?.Value ?? throw new ModAttributeException($"Missing {attributeName} in " + GetModIdentityString());
            }

            // Parse an optional string from XML doc
            string? ParseOptionalString(string attributeName)
            {
                return xmlDocument.Attribute(attributeName)?.Value;
            }

            // Parse a required version from XML doc
            Version ParseVersion(string attributeName)
            {
                var value = ParseString(attributeName);

                try { return Version.Parse(value); }
                catch (Exception e) { throw new ModAttributeException($"Invalid {attributeName} {value} in " + GetModIdentityString(), e); }
            }

            // Parse an optional version from XML doc
            Version? ParseOptionalVersion(string attributeName)
            {
                var value = ParseOptionalString(attributeName);

                if (value is null) return null;

                try { return Version.Parse(value); }
                catch (Exception e) { throw new ModAttributeException($"Invalid {attributeName} {value} in " + GetModIdentityString(), e); }
            }

            // Parse a required boolean from XML doc
            bool ParseBool(string attributeName)
            {
                var value = ParseString(attributeName);

                try { return bool.Parse(value); }
                catch (Exception e) { throw new ModAttributeException($"Invalid {attributeName} {value} (must be true or false) in " + GetModIdentityString(), e); }
            }

            // Parse an optional boolean (defaulting to false) from XML doc
            bool ParseOptionalBool(string attributeName)
            {
                var value = ParseString(attributeName);

                if (value is null) return false;

                try { return bool.Parse(value); }
                catch (Exception e) { throw new ModAttributeException($"Invalid {attributeName} {value} (must be true or false) in " + GetModIdentityString(), e); }
            }


            InstallerSystemVersion = ParseVersion("installerSystemVersion");

            Unique = ParseString("unique");
            DisplayName = ParseString("displayName");
            Description = ParseOptionalString("displayName");

            ModVersion = ParseOptionalVersion("modVersion"); // Prerelease, subject to change
            DllsBuild = ParseVersion("dllsBuild");

            CanDisableMod = ParseOptionalBool("canDisableMod"); // Prerelease, subject to change
            HasCustomInstaller = ParseOptionalBool("hasCustomInstaller"); // Does not exist in InstallerSystemVersion 1.0.0.0

            IsExperimental = ParseOptionalBool("isExperimental");
            RequiresGalaxyReset = ParseOptionalBool("requiresGalaxyReset");
            CausesSaveDataDependency = ParseOptionalBool("causesSaveDataDependency");
        }
    }
}
