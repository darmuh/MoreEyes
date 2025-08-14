using HarmonyLib;
using MoreEyes.EyeManagement;
using MoreEyes.Menus;
using System.Collections.Generic;
using UnityEngine;

namespace MoreEyes.Core;

[HarmonyPatch(typeof(PlayerAvatarVisuals), nameof(PlayerAvatarVisuals.Start))]
internal class LocalPlayerMenuPatch
{
    public static void Postfix(PlayerAvatarVisuals __instance)
    {
        if (!__instance.isMenuAvatar)
            return;

        Plugin.Spam("Getting menu player eyes");
        CustomEyeManager.AllPatchedEyes.RemoveAll(p => p.Player == null);
        PatchedEyes.Local.GetPlayerMenuEyes(__instance);
    }
}

//custom assets could probably be loaded before spawn
//however, vanilla references will need to be created at first spawn
[HarmonyPatch(typeof(PlayerAvatar), nameof(PlayerAvatar.Spawn))]
internal class PlayerSpawnPatch
{
    public static void Postfix(PlayerAvatar __instance)
    {
        CustomEyeManager.CheckForVanillaPupils();

        // Placed this here now that we are only initializing types once
        Plugin.Spam($"Player ({__instance.playerName}) spawned, updating their eyes!");
        GetPlayerEyes(__instance);

    }

    internal static void GetPlayerEyes(PlayerAvatar player)
    {
        Plugin.Spam($"GetPlayerEyes for {player.playerName}");
        if (!PlayerEyeSelection.TryGetSelections(player.steamID, out PlayerEyeSelection selections))
            selections = new(player.steamID);

        PatchedEyes patchedEyes = PatchedEyes.GetPatchedEyes(PlayerAvatar.instance);

        //link these two
        patchedEyes.currentSelections = selections;
        selections.patchedEyes = patchedEyes;

        if (player.isLocal)
        {
            Plugin.Spam("No need to change local player's eyes for player object. They can't see them.");
            return;
        }

        patchedEyes.SetSelectedEyes(player);
    }
}

[HarmonyPatch(typeof(MenuPageEsc), nameof(MenuPageEsc.Update))]
internal class MenuEscPatch
{
    private static GameObject target;
    public static void Postfix(MenuPageEsc __instance)
    {
        if (Menu.MoreEyesMenu.menuPage != null)
        {
            Transform playerAvatar = __instance.transform.Find("Menu Element Player Avatar");
            if (playerAvatar != null)
            {
                target = playerAvatar.gameObject;
            }
            if (target != null && target.activeSelf)
            {
                target.SetActive(false);
            }
        }
        else
        {
            if (target != null && !target.activeSelf)
            {
                target.SetActive(true);
            }
        }
    }
}