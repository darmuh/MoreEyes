using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MoreEyes
{
    public class CustomEyeType
    {
        public string Name = string.Empty;
        public GameObject Iris = null!;
        internal bool isVanilla = false;

        public CustomEyeType()
        {
        }

        public void AddCustomEyes(string assetName)
        {
            Name = assetName;
            Iris = Plugin.Assets.LoadAsset<GameObject>(assetName);
            if (Iris == null)
            {
                Plugin.logger.LogWarning($"IRIS IS NULL FOR ASSETNAME - [ {assetName} ]");
            }
            Plugin.AllEyeTypes.Add(this);
            Plugin.AllEyeTypes.Distinct();
        }

        internal void AddVanillaEyes(string name, GameObject eyeObject)
        {
            Name = name;
            Iris = eyeObject;
        }

        internal static void GetAllTypes()
        {
            //List asset names here to create custom types all at once
            List<string> Names = ["cat_iris", "diamond_iris", "cross_iris"];

            Names.Do(n =>
            {
                CustomEyeType thisType = new();
                thisType.AddCustomEyes(n);
            });

            //placed this here so that all types are loaded before updating
            Plugin.Spam("Player spawned, updating irises for all players!");
            List<PlayerAvatar> allPlayers = SemiFunc.PlayerGetAll();

            allPlayers.Do(p => PlayerSpawnPatch.GetPlayerEyes(p));
        }
    }
}
