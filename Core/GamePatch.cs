using HarmonyLib;
using MoreEyes.EyeManagement;
using System.Collections.Generic;
using MoreEyes.Menus;
using UnityEngine;

namespace MoreEyes.Core;

[HarmonyPatch(typeof(PlayerAvatarVisuals), nameof(PlayerAvatarVisuals.Start))]
public class LocalPlayerMenuPatch
{
    public static void Postfix(PlayerAvatarVisuals __instance)
    {
        if (!__instance.isMenuAvatar)
            return;

        Plugin.Spam("Getting menu player eyes, local player can't see their own pupils");

        CustomEyeManager.AllPatchedEyes.RemoveAll(p => p.playerRef == null);
        PatchedEyes patchedEyes = PatchedEyes.GetPatchedEyes(PlayerAvatar.instance);

        patchedEyes.GetPlayerMenuEyes(__instance);
        PatchedEyes.SetLocalEyes();

        //patchedEyes.RandomizeEyes(PlayerAvatar.instance.playerName, pupilLeft, pupilRight);
    }
}

//custom assets could probably be loaded before spawn
//however, vanilla references will need to be created at first spawn
[HarmonyPatch(typeof(PlayerAvatar), nameof(PlayerAvatar.Spawn))]
public class PlayerSpawnPatch
{
    public static void Postfix()
    {
        CustomEyeManager.CheckForVanillaPupils();

        // Placed this here now that we are only initializing types once
        Plugin.Spam("Player spawned, updating pupils for all players!");
        List<PlayerAvatar> allPlayers = SemiFunc.PlayerGetAll();
        Plugin.Spam($"{allPlayers.Count} players detected");

        allPlayers.Do(p => GetPlayerEyes(p));

    }

    internal static void GetPlayerEyes(PlayerAvatar player)
    {
        Plugin.Spam($"GetPlayerEyes for {player.playerName}");
        PlayerEyeSelection selections = new(player.steamID);
        PatchedEyes patchedEyes = PatchedEyes.GetPatchedEyes(PlayerAvatar.instance);
        
        //link these two
        patchedEyes.playerSelections = selections;
        selections.patchedEyes = patchedEyes;

        if (player.isLocal)
        {
            Plugin.Spam("No need to change local player's eyes for player object. They can't see them.");
            return;
        }

        // UpdateObjectRefs playervisual eyes
        //patchedEyes.RandomizeEyes(player);

        patchedEyes.SetSelectedEyes(player);
    }
}

[HarmonyPatch(typeof(MenuPageEsc), nameof(MenuPageEsc.Update))]
public class MenuEscPatch
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