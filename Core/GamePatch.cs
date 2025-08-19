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
        if (__instance.isMenuAvatar)
        {
            if (PlayerAvatar.instance == null)
                return;

            Plugin.Spam("Getting local player menu eye references");
            PatchedEyes.Local.SetMenuEyes(__instance);
        }
        else
        {
            if(string.IsNullOrEmpty(__instance.playerAvatar.steamID))
            {
                Plugin.logger.LogError("Unknown steamID for player!");
                return;
            }

            var patchedEyes = CustomEyeManager.AllPatchedEyes.Find(p => p.playerID == __instance.playerAvatar.steamID);

            if (patchedEyes == null)
            {
                Plugin.WARNING($"Could not get PatchedEyes for player {__instance.playerAvatar.playerName}");
                return;
            }   

            if (patchedEyes.LeftEye == null || patchedEyes.RightEye == null)
            {
                Plugin.WARNING($"Eye references are null for {patchedEyes.Player.playerName}");
                return;
            }

            //set player selections!!!
            patchedEyes.currentSelections.PlayerEyesSpawn();
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
        GetPlayerEyes(__instance);
    }

    internal static void GetPlayerEyes(PlayerAvatar player)
    {
        Plugin.Spam($"GetPlayerEyes for {player.playerName}");
        if (!PlayerEyeSelection.TryGetSelections(player.steamID, out PlayerEyeSelection selections))
            selections = new(player.steamID);

        PatchedEyes patchedEyes = player.AddComponent<PatchedEyes>();

        //link these two for easy back and forth
        patchedEyes.currentSelections = selections;
        selections.patchedEyes = patchedEyes;
        patchedEyes.SetPlayerSavedSelection(player);
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
            Transform playerAvatar = __instance.transform.Find("Menu Element Player Avatar");
            if (playerAvatar != null)
            {
                playerTarget = playerAvatar.gameObject;
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