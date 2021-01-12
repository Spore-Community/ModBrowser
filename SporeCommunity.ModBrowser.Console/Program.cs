using SporeCommunity.ModBrowser.GithubApi;
using System;

var github = new GithubModSearchEngine("Spore-Community/ModBrowser");

var mods = await github.SearchModsAsync();

Console.WriteLine($"Found {mods.Count} mods.");

foreach (var mod in mods)
{
    Console.WriteLine(mod.DisplayName+" by "+mod.Author);
}