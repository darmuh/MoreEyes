using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MoreEyes.EyeManagement;
using MoreEyes.Menus;
using System.IO;
using System.Reflection;

namespace MoreEyes.Core;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("nickklmao.menulib", BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency("soundedsquash.spawnmanager", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("TwitchChatAPI.REPO", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("com.github.zehsteam.TwitchTrolling", BepInDependency.DependencyFlags.SoftDependency)]
internal class Plugin : BaseUnityPlugin
{
    internal static ManualLogSource logger;
    internal static System.Random Rand = new();

    public void Awake()
    {
        logger = Logger;
        string pluginFolderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string assetBundleFilePath = Path.Combine(pluginFolderPath, "eyes");
        AssetManager.DefaultAssets = AssetManager.InitBundle(assetBundleFilePath);
        Menu.Initialize();
        CustomEyeManager.Init();

        Spam("Plugin initialized!");
        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
    }

    internal static void Spam(string message)
    {
        // Config item to disable dev logging here
        logger.LogDebug(message);
    }

    internal static void WARNING(string message)
    {
        logger.LogWarning(message);
    }
}