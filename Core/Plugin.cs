using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MoreEyes.EyeManagement;
using MoreEyes.Menus;
using System.Reflection;

namespace MoreEyes.Core;
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency(ModCompats.MenuLib_PLUGIN_GUID, BepInDependency.DependencyFlags.HardDependency)]
[BepInDependency(ModCompats.SpawnManager_PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(ModCompats.TwitchChatAPI_PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(ModCompats.TwitchTrolling_PLUGIN_GUID, BepInDependency.DependencyFlags.SoftDependency)]
internal class Plugin : BaseUnityPlugin
{
    internal static System.Random Rand = new();
    internal static new ManualLogSource Logger { get; private set; }
    public void Awake()
    {
        Logger = BepInEx.Logging.Logger.CreateLogSource(MyPluginInfo.PLUGIN_GUID);

        AssetManager.InitBundles();
        Menu.Initialize();
        CustomEyeManager.Init();

        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
    }
}