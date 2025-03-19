using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MoreEyes
{
    public class CustomEyeType
    {
        public string Name = string.Empty;
        public GameObject Pupil = null!;
        public GameObject Iris = null!;
        internal bool isVanilla = false;

        internal static List<string> UsedPupilNames = [];
        public CustomEyeType()
        {
        }

        public void AddCustomEyes(string assetName)
        {
            Name = assetName;
            Pupil = Plugin.Assets.LoadAsset<GameObject>(assetName);
            if (Pupil == null)
            {
                Plugin.logger.LogWarning($"PUPIL IS NULL FOR ASSETNAME - [ {assetName} ]");
            }
            Plugin.AllEyeTypes.Add(this);
            Plugin.AllEyeTypes.Distinct();

            if (!UsedPupilNames.Contains(assetName))
            {
                UsedPupilNames.Add(assetName);
            }
        }

        internal void AddVanillaEyes(string name, GameObject eyeObject)
        {
            Name = name;
            Pupil = eyeObject;
        }

        internal static void GetAllTypes()
        {
            // List asset names here to create custom types all at once
            // For these irises we need to make sure we are not replacing the vanilla pupil just adding them in as childobjects, which shouldnt take long
            List<string> irisNames = ["cat_iris", "diamond_iris", "square_iris", "ring_iris"];
            List<string> pupilNames = ["cat_pupil", "cockade_pupil", "cross_pupil", "diamond_pupil", "kawaii_pupil", "x_pupil"];

            pupilNames.Do(n =>
            {
                CustomEyeType thisType = new();
                thisType.AddCustomEyes(n);
            });

            // Placed this here so that all types are loaded before updating
            Plugin.Spam("Player spawned, updating pupils for all players!");
            List<PlayerAvatar> allPlayers = SemiFunc.PlayerGetAll();

            allPlayers.Do(p => PlayerSpawnPatch.GetPlayerEyes(p));
        }

        public static void HotReloadAsset()
        {
            // Unload unused assets from memory and reload them when necessary (this is not finished yet - go ham at it if you want to)
            Resources.UnloadUnusedAssets();

            Plugin.AllEyeTypes.Clear();

            foreach (var usedPupil in UsedPupilNames)
            {
                CustomEyeType used = new();
                used.AddCustomEyes(usedPupil);
                Plugin.logger.LogInfo($"Reloaded {usedPupil}");

                List<PlayerAvatar> allPlayers = SemiFunc.PlayerGetAll();
                allPlayers.ForEach(p => PlayerSpawnPatch.GetPlayerEyes(p)); // Reapply new eyes to all players
            }

        }
        public static void MarkPupilsAsUnused(string unusedPupils)
        {
            if (UsedPupilNames.Contains(unusedPupils))
            {
                UsedPupilNames.Remove(unusedPupils);
                Plugin.logger.LogInfo($"{unusedPupils} was marked as unused."); // We can get rid of this logger in the future
            }
        }
    }
}
