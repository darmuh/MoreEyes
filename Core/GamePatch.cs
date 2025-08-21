using HarmonyLib;
using MoreEyes.EyeManagement;
using MoreEyes.Menus;
using Unity.VisualScripting;
using UnityEngine;

namespace MoreEyes.Core;

[HarmonyPatch(typeof(RunManagerPUN), nameof(RunManagerPUN.Start))]
internal class NetworkComponentPatch
{
    public static void Postfix(RunManagerPUN __instance)
    {
        if(MoreEyesNetwork.instance == null)
            __instance.AddComponent<MoreEyesNetwork>();

        if(PlayerEyeSelection.LocalCache == null)
            _ = new PlayerEyeSelection(true);
    }
}

//This patch actually hooks in to replace all default eyes
[HarmonyPatch(typeof(PlayerAvatarVisuals), nameof(PlayerAvatarVisuals.Start))]
internal class LocalPlayerMenuPatch
{
    public static void Postfix(PlayerAvatarVisuals __instance)
    {
        if (PlayerAvatar.instance == null || SemiFunc.SplashScreenLevel())
            return;

        if (__instance.isMenuAvatar)
        {
            Plugin.Spam("Getting local player menu eye references");
            PatchedEyes.Local.SetMenuEyes(__instance);
        }
    }
}

//Moved to PlayerEyes Start because well, we're only worried about eyes lol
[HarmonyPatch(typeof(PlayerAvatar), nameof(PlayerAvatar.SpawnRPC))]
internal class PlayerSpawnPatch
{
    public static void Postfix(PlayerAvatar __instance)
    {
        Plugin.Spam($"Player ({__instance.playerName}) spawned, updating their eyes!");
        PatchedEyes.GetPatchedEyes(__instance);
    }
}

[HarmonyPatch(typeof(MenuPageEsc), nameof(MenuPageEsc.Update))]
internal class MenuEscPatch
{
    private static GameObject playerTarget;
    public static void Postfix(MenuPageEsc __instance)
    {
        if (Menu.MoreEyesMenu.menuPage != null)
        {
            if(playerTarget == null) //only find transform if we need to
            {
                Transform playerAvatar = __instance.transform.Find("Menu Element Player Avatar");
                if (playerAvatar != null)
                {
                    playerTarget = playerAvatar.gameObject;
                }
            }
            
            if (playerTarget != null && playerTarget.activeSelf)
            {
                playerTarget.SetActive(false);
            }
        }
        else
        {
            if (playerTarget != null && !playerTarget.activeSelf)
            {
                playerTarget.SetActive(true);
            }
        }
    }
}