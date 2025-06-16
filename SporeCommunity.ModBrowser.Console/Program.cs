using SporeCommunity.ModBrowser.GithubApi;
using System;

var github = new GithubModSearchEngine("Spore-Community/ModBrowser");

var mods = github.SearchModsAsync();

//Console.WriteLine($"Found {mods.Count} mods.");

await foreach (var mod in mods)
{
    Console.WriteLine(mod.DisplayName+" by "+mod.Author);
}