using HarmonyLib;
using MoreEyes.EyeManagement;
using System.Collections.Generic;

namespace MoreEyes.Core
{
    [HarmonyPatch(typeof(PlayerAvatarVisuals), "Start")]
    public class LocalPlayerMenuPatch
    {
        public static void Postfix(PlayerAvatarVisuals __instance)
        {
            if (!__instance.isMenuAvatar)
                return;

            Plugin.Spam("Getting menu player eyes, local player can't see their own pupils");
            //Transform pupilLeft = __instance.playerEyes.pupilLeft;
            //Transform pupilRight = __instance.playerEyes.pupilRight;

            //CustomEyeManager.AllPatchedEyes.RemoveAll(p => p.playerRef == null);
            //PatchedEyes patchedEyes = PatchedEyes.GetPatchedEyes(PlayerAvatar.instance);

            //patchedEyes.RandomizeEyes(PlayerAvatar.instance.playerName, pupilLeft, pupilRight);
        }
    }

    //custom assets could probably be loaded before spawn
    //however, vanilla references will need to be created at first spawn
    [HarmonyPatch(typeof(PlayerAvatar), "Spawn")]
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
            patchedEyes.RandomizeEyes(player);
        }
    }

}
