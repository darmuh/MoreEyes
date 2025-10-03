using BepInEx.Bootstrap;

namespace MoreEyes.Utility;
internal static class ModCompats
{
    internal const string MenuLib_PLUGIN_GUID = "nickklmao.menulib";
    internal const string SpawnManager_PLUGIN_GUID = "soundedsquash.spawnmanager";
    internal const string TwitchChatAPI_PLUGIN_GUID = "TwitchChatAPI.REPO";
    internal const string TwitchTrolling_PLUGIN_GUID = "com.github.zehsteam.TwitchTrolling";
    internal const string MoreHead_PLUGIN_GUID = "Mhz.REPOMoreHead";
    internal static bool IsSpawnManagerPresent => Chainloader.PluginInfos.ContainsKey(SpawnManager_PLUGIN_GUID);
    internal static bool IsTwitchChatAPIPresent => Chainloader.PluginInfos.ContainsKey(TwitchChatAPI_PLUGIN_GUID);
    internal static bool IsTwitchTrollingPresent => Chainloader.PluginInfos.ContainsKey(TwitchTrolling_PLUGIN_GUID);
    private static bool? _morehead = null!;
    internal static bool IsMoreHeadPresent()
    {
        _morehead ??= Chainloader.PluginInfos.ContainsKey(MoreHead_PLUGIN_GUID);

        return _morehead.Value;
    }
}