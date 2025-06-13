using BepInEx.Bootstrap;

namespace MoreEyes.Core.ModCompats;

internal static class ModCompats
{
    internal static bool IsSpawnManagerPresent => Chainloader.PluginInfos.ContainsKey("soundedsquash.spawnmanager");
    internal static bool IsTwitchChatAPIPresent => Chainloader.PluginInfos.ContainsKey("TwitchChatAPI.REPO");
    internal static bool IsTwitchTrollingPresent => Chainloader.PluginInfos.ContainsKey("com.github.zehsteam.TwitchTrolling");
}