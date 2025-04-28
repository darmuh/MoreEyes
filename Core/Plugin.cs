using BepInEx;
using HarmonyLib;
using System.IO;
using BepInEx.Logging;
using System.Reflection;
using MoreEyes.Menus;
using MoreEyes.EyeManagement;

namespace MoreEyes.Core;

// Logger class
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("nickklmao.menulib", BepInDependency.DependencyFlags.HardDependency)]
public class Plugin : BaseUnityPlugin
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
}