using System;
using System.Linq;
using UnityEngine;

namespace MoreEyes
{
    public class CustomIrisType(string path)
    {
        public string Name = path;
        public GameObject Iris = null!;

        //easier to go through lists in UnityExplorer
        public override string ToString()
        {
            return Name;
        }

        public void AddCustomIris()
        {
            //See AddCustomPupils comments
            Name = Name[(Name.LastIndexOf('/') + 1)..].Replace(".prefab", "");

            AssetManager.DefaultAssets.LoadAssetGameObject(Name, out Iris);
            if (Iris == null)
                Plugin.logger.LogWarning($"IRIS IS NULL FOR ASSETNAME - [ {Name} ]");

            Iris.SetActive(false);
            CustomEyeManager.AllIrisTypes.Add(this);
            CustomEyeManager.AllIrisTypes.Distinct();
        }


        //note sure if we are gonna use this to be honest
        public void MarkIrisUnused()
        {
            if (CustomEyeManager.IrisInUse.Contains(this))
            {
                CustomEyeManager.IrisInUse.Remove(this);
                Plugin.logger.LogInfo($"{Name} was marked as unused."); // We can get rid of this logger in the future
            }
        }
    }
}
