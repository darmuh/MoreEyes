using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MoreEyes.EyeManagement;
using MoreEyes.Menus;
using System.IO;
using System.Reflection;

namespace MoreEyes.Core;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(ModCompats.MenuLib_PLUGIN_GUID, BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency(ModCompats.SpawnManager_PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(ModCompats.TwitchChatAPI_PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(ModCompats.TwitchTrolling_PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
internal class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource logger;
    internal static System.Random Rand = new();
    public void Awake()
    {
        logger = Logger;
        AssetManager.InitBundles();
        Menu.Initialize();
        CustomEyeManager.Init();

        Spam("Plugin initialized!");

        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
    }

    internal static void Spam(string message)
    {
        logger.LogDebug(message);
    }

    internal static void WARNING(string message)
    {
        logger.LogWarning(message);
    }
}