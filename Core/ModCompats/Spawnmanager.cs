using BepInEx.Bootstrap;

namespace MoreEyes.Core.ModCompats;

internal static class Spawnmanager
{
    internal static bool IsSpawnManagerPresent => Chainloader.PluginInfos.ContainsKey("soundedsquash.spawnmanager");
}