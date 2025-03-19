using UnityEngine;
using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using BepInEx.Logging;
using System.Reflection;
using System.Linq;

namespace MoreEyes
{
    // Logger class
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource logger;
        internal static AssetBundle Assets;
        internal static List<PatchedEyes> AllPatchedEyes = [];
        internal static List<CustomEyeType> AllEyeTypes = [];

        public void Awake()
        {
            logger = base.Logger;
            string pluginFolderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string assetBundleFilePath = Path.Combine(pluginFolderPath, "eyes");
            Assets = AssetBundle.LoadFromFile(assetBundleFilePath);
            
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
            PatchedEyes patchedEyes = PlayerAvatar.instance.gameObject.GetComponent<PatchedEyes>() ?? PlayerAvatar.instance.gameObject.AddComponent<PatchedEyes>();
            patchedEyes.playerID = PlayerAvatar.instance.steamID;


            // Have to reset each time menu is opened unfortunately
            patchedEyes.selectedLeft = null!;
            patchedEyes.selectedRight = null!;

            patchedEyes.CommonEyeMethod(PlayerAvatar.instance.playerName, pupilLeft, pupilRight);
        }
    }


    [HarmonyPatch(typeof(PlayerAvatar), "Spawn")]
    public class PlayerSpawnPatch
    {
        public static void Postfix(PlayerAvatar __instance)
        {
            if (SemiFunc.RunIsLobbyMenu() || RunManager.instance.levelCurrent == RunManager.instance.levelMainMenu)
                return;

            Plugin.AllEyeTypes.RemoveAll(t => t == null);

            // Keep reference to vanilla eyes type
            GetVanilla(__instance);

            // Get all custom eye types
            CustomEyeType.GetAllTypes();
        }
        
        private static void GetVanilla(PlayerAvatar player)
        {
            if (!Plugin.AllEyeTypes.Any(t => t.isVanilla))
            {
                CustomEyeType vanillaLeft = new()
                {
                    isVanilla = true
                };
                vanillaLeft.AddVanillaEyes("vanillaLeft", player.playerAvatarVisuals.playerEyes.pupilLeft.GetChild(0).gameObject);

                CustomEyeType vanillaRight = new()
                {
                    isVanilla = true
                };
                vanillaRight.AddVanillaEyes("vanillaRight", player.playerAvatarVisuals.playerEyes.pupilRight.GetChild(0).gameObject);
            }
        }

        internal static void GetPlayerEyes(PlayerAvatar player)
        {
            Plugin.Spam($"GetPlayerEyes for {player.playerName}");

            if (player.isLocal)
            {
                Plugin.Spam("No need to change local player's eyes for player object. They can't see them.");
                return;
            }

            PatchedEyes patchedEyes = player.gameObject.GetComponent<PatchedEyes>() ?? player.gameObject.AddComponent<PatchedEyes>();
            patchedEyes.playerID = player.steamID;

            // UpdateObjectRefs playervisual eyes
            Transform pupilLeft = player.playerAvatarVisuals.playerEyes.pupilLeft;
            Transform pupilRight = player.playerAvatarVisuals.playerEyes.pupilRight;
            patchedEyes.CommonEyeMethod(player.playerName, pupilLeft, pupilRight);
        }
    }
}
