using HarmonyLib;
using MoreEyes.EyeManagement;
using MoreEyes.Menus;
using Unity.VisualScripting;
using UnityEngine;

namespace MoreEyes.Core;

[HarmonyPatch(typeof(PlayerAvatarVisuals), nameof(PlayerAvatarVisuals.Start))]
internal class LocalPlayerMenuPatch
{
    public static void Postfix(PlayerAvatarVisuals __instance)
    {
        if (!__instance.isMenuAvatar)
            return;

        Plugin.Spam("Getting local player menu eye references");
        PatchedEyes.Local.SetMenuEyes(__instance);
    }
}

//custom assets could probably be loaded before spawn
//however, vanilla references will need to be created at first spawn
[HarmonyPatch(typeof(PlayerAvatar), nameof(PlayerAvatar.SpawnRPC))]
internal class PlayerSpawnPatch
{
    public static void Postfix(PlayerAvatar __instance)
    {
        // Placed this here now that we are only initializing types once
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
        patchedEyes.SetSelectedEyes(player);
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