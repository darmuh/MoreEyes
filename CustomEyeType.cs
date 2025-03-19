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
        }

        internal void AddVanillaEyes(string name, GameObject eyeObject)
        {
            Name = name;
            Pupil = eyeObject;
        }

        internal static void GetAllTypes()
        {
            //List asset names here to create custom types all at once
            //For these irises we need to make sure we are not replacing the vanilla pupil just adding them in as childobjects, which shouldnt take long
            List<string> irisNames = ["cat_iris", "diamond_iris", "square_iris", "ring_iris"];
            List<string> pupilNames = ["cat_pupil", "cockade_pupil", "cross_pupil", "diamond_pupil", "kawaii_pupil", "x_pupil"];

            pupilNames.Do(n =>
            {
                CustomEyeType thisType = new();
                thisType.AddCustomEyes(n);
            });

            //placed this here so that all types are loaded before updating
            Plugin.Spam("Player spawned, updating pupils for all players!");
            List<PlayerAvatar> allPlayers = SemiFunc.PlayerGetAll();

            allPlayers.Do(p => PlayerSpawnPatch.GetPlayerEyes(p));
        }
    }
}
