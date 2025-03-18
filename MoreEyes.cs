using UnityEngine;
using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;

namespace MoreEyes
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [HarmonyPatch(typeof(PlayerAvatar), "Spawn")]
    public class PlayerSpawnPatch : BaseUnityPlugin
    {
        private static List<CustomEyePatch> customEyePatches = [];
        public void Awake()
        {
            string pluginFolderPath = Path.GetDirectoryName(Info.Location);
            string assetBundleFilePath = Path.Combine(pluginFolderPath, "eyes");
            AssetBundle assetBundle = AssetBundle.LoadFromFile(assetBundleFilePath);
            GameObject cat_iris = assetBundle.LoadAsset<GameObject>("cat_iris");
            CustomEyePatch catEyePatch = new(cat_iris, cat_iris);
            customEyePatches.Add(catEyePatch);
        }
        public static void Postfix(PlayerAvatar __instance)
        {
            if (SemiFunc.RunIsLobbyMenu() || RunManager.instance.levelCurrent == RunManager.instance.levelMainMenu)
                return;

            List<PlayerAvatar> allPlayers = SemiFunc.PlayerGetAll();

            allPlayers.Do(p => GetPlayerEyes(p));
        }
        internal static void GetPlayerEyes(PlayerAvatar player)
        {
            Transform pupilLeft = player.playerAvatarVisuals.playerEyes.pupilLeft;
            Transform pupilRight = player.playerAvatarVisuals.playerEyes.pupilRight;

            if (pupilLeft.childCount == 0 || pupilRight.childCount == 0)
                return;

            GameObject leftPupilObject = pupilLeft.GetChild(0).gameObject;
            GameObject rightPupilObject = pupilRight.GetChild(0).gameObject;

            foreach (CustomEyePatch customEyePatch in customEyePatches)
            {
                customEyePatch.ReplaceEyePatches(customEyePatch.LeftPupilObject, customEyePatch.RightPupilObject);
            }
        }
    }
}
