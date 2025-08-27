﻿using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MoreEyes.Managers;
using MoreEyes.Utility;
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

    internal const string moreEyesAscii = @$"
 _   .-')                _  .-')     ('-.           ('-.                 ('-.    .-')    
( '.( OO )_             ( \( -O )  _(  OO)        _(  OO)              _(  OO)  ( OO ).  
 ,--.   ,--.).-'),-----. ,------. (,------.      (,------. ,--.   ,--.(,------.(_)---\_) 
 |   `.'   |( OO'  .-.  '|   /`. ' |  .---'       |  .---'  \  `.'  /  |  .---'/    _ |  
 |         |/   |  | |  ||  /  | | |  |           |  |    .-')     /   |  |    \  :` `.  
 |  |'.'|  |\_) |  |\|  ||  |_.' |(|  '--.       (|  '--.(OO  \   /   (|  '--.  '..`''.) 
 |  |   |  |  \ |  | |  ||  .  '.' |  .--'        |  .--' |   /  /\_   |  .--' .-._)   \ 
 |  |   |  |   `'  '-'  '|  |\  \  |  `---.       |  `---.`-./  /.__)  |  `---.\       / 
 `--'   `--'     `-----' `--' '--' `------'       `------'  `--'       `------' `-----'  
";
    public void Awake()
    {
        Logger = BepInEx.Logging.Logger.CreateLogSource(MyPluginInfo.PLUGIN_GUID);
        ModConfig.BindConfigItems(Config);

        EyesAssetManager.InitBundles();
        EyeSelectionMenu.Initialize();
        ExpressionMenu.Initialize();
        CustomEyeManager.Init();

        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        Loggers.Message($"{MyPluginInfo.PLUGIN_NAME} has been loaded with version - {MyPluginInfo.PLUGIN_VERSION}");
        Loggers.Message(moreEyesAscii);
    }
}