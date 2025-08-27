using HarmonyLib;
using MoreEyes.Collections;
using MoreEyes.Components;
using MoreEyes.Utility;
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

[HarmonyPatch(typeof(PlayerAvatarVisuals), nameof(PlayerAvatarVisuals.Start))]
internal class LocalPlayerMenuPatch
{
    public static void Postfix(PlayerAvatarVisuals __instance)
    {
        if (PlayerAvatar.instance == null || SemiFunc.SplashScreenLevel())
            return;

        if (__instance.isMenuAvatar)
        {
            PatchedEyes.Local.SetMenuEyes(__instance);
        }
    }
}

[HarmonyPatch(typeof(PlayerAvatar), nameof(PlayerAvatar.SpawnRPC))]
internal class PlayerSpawnPatch
{
    public static void Postfix(PlayerAvatar __instance)
    {
        PatchedEyes.GetPatchedEyes(__instance);
    }
}

[HarmonyPatch(typeof(MenuPageEsc), nameof(MenuPageEsc.Update))]
internal class MenuEscPatch
{
    private static GameObject playerTarget;
    private static Transform playerAvatar;
    public static void Postfix(MenuPageEsc __instance)
    {
        if (EyeSelectionMenu.MoreEyesMenu.menuPage != null)
        {
            if(playerTarget == null)
            {
                playerAvatar = __instance.transform.Find("Menu Element Player Avatar");
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