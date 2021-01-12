using System;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SporeCommunity.ModBrowser.GithubApi
{
    public class Repository
    {
        private readonly GithubApiClient client;

        // TODO: add ctor that accepts the JSON response from the GH API

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
