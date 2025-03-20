using UnityEngine;
using BepInEx;
using HarmonyLib;
using System.IO;
using BepInEx.Logging;
using System.Reflection;
using System.Linq;
using Photon.Realtime;
using System.Collections.Generic;

namespace MoreEyes
{
    // Logger class
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource logger;
        internal static System.Random Rand = new();

        public void Awake()
        {
            logger = base.Logger;
            string pluginFolderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string assetBundleFilePath = Path.Combine(pluginFolderPath, "eyes");
            AssetManager.DefaultAssets = AssetManager.InitBundle(assetBundleFilePath);
            
            Spam("Plugin initialized!");
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        }

        internal static void Spam(string message)
        {
            // Config item to disable dev logging here
            logger.LogDebug(message);
        }
    }

    [HarmonyPatch(typeof(PlayerAvatarVisuals), "Start")]
    public class LocalPlayerMenuPatch
    {
        public static void Postfix(PlayerAvatarVisuals __instance)
        {
            if (!__instance.isMenuAvatar)
                return;

            Plugin.Spam("Getting menu player eyes, local player can't see their own pupils");
            Transform pupilLeft = __instance.playerEyes.pupilLeft;
            Transform pupilRight = __instance.playerEyes.pupilRight;

            CustomEyeManager.AllPatchedEyes.RemoveAll(p => p.playerRef == null);
            PatchedEyes patchedEyes = PatchedEyes.GetPatchedEyes(PlayerAvatar.instance);
            patchedEyes.CommonEyeMethod(PlayerAvatar.instance.playerName, pupilLeft, pupilRight);
        }
    }


    [HarmonyPatch(typeof(PlayerAvatar), "Spawn")]
    public class PlayerSpawnPatch
    {
        public static void Postfix()
        {
            //allowing for lobby menu since you can change color in lobby menu
            if (RunManager.instance.levelCurrent == RunManager.instance.levelMainMenu)
                return;

            CustomEyeManager.AllIrisTypes.RemoveAll(t => t == null);
            CustomEyeManager.AllPupilTypes.RemoveAll(t => t == null);

            // Get all custom eye types
            // need to clear lists to not create duplicates
            // CustomEyeManager.ClearLists();
            // OR
            // We set an initialization bool and only load types once!
            if (!CustomEyeManager.isInitialized)
            {
                AssetManager.LoadedAssets.Do(asset =>
                {
                    //This will go through any assets that have been registered with our mod
                    CustomEyeManager.GetAllTypes(asset);
                });
                
                Plugin.Spam("CustomEyeManager Initialized!");
            }

            // Placed this here now that we are only initializing types once
            Plugin.Spam("Player spawned, updating pupils for all players!");
            List<PlayerAvatar> allPlayers = SemiFunc.PlayerGetAll();
            Plugin.Spam($"{allPlayers.Count} players detected");

            allPlayers.Do(p => GetPlayerEyes(p));

        }

        internal static void GetPlayerEyes(PlayerAvatar player)
        {
            Plugin.Spam($"GetPlayerEyes for {player.playerName}");

            PatchedEyes patchedEyes = PatchedEyes.GetPatchedEyes(PlayerAvatar.instance);

            if (player.isLocal)
            {
                Plugin.Spam("No need to change local player's eyes for player object. They can't see them.");
                return;
            }

            // UpdateObjectRefs playervisual eyes
            Transform pupilLeft = player.playerAvatarVisuals.playerEyes.pupilLeft;
            Transform pupilRight = player.playerAvatarVisuals.playerEyes.pupilRight;
            patchedEyes.CommonEyeMethod(player.playerName, pupilLeft, pupilRight);
        }
    }
}
