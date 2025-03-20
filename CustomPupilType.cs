
using System.Linq;
using UnityEngine;

namespace MoreEyes
{
    public class CustomPupilType(string path)
    {
        public string Name = path;
        public GameObject Pupil = null!;
        internal bool isVanilla = false;

        //internal static List<string> UsedPupilNames = [];

        //easier to go through lists in UnityExplorer
        public override string ToString()
        {
            return Name;
        }

        public void AddCustomPupils()
        {
            // the method to get the asset names dynamically includes the whole path
            // this will replace the string we've assigned without the path at the front
            // it also replaces ".prefab" at the end with nothing
            Name = Name[(Name.LastIndexOf('/') + 1)..].Replace(".prefab", "");

            AssetManager.DefaultAssets.LoadAssetGameObject(Name, out Pupil);
            if (Pupil == null)
                Plugin.logger.LogWarning($"PUPIL IS NULL FOR ASSETNAME - [ {Name} ]");
            Pupil.SetActive(false);
            CustomEyeManager.AllPupilTypes.Add(this);
            CustomEyeManager.AllPupilTypes.Distinct();
        }

        internal void AddVanillaEye(GameObject eyeObject)
        {
            Pupil = GameObject.Instantiate(eyeObject);
            Pupil.SetActive(false);
            isVanilla = true;
            CustomEyeManager.AllPupilTypes.Add(this);
        }

        public void MarkPupilUnused()
        {
            if (CustomEyeManager.PupilsInUse.Contains(this))
            {
                CustomEyeManager.PupilsInUse.Remove(this);
                Plugin.logger.LogInfo($"{Name} was marked as unused."); // We can get rid of this logger in the future
            }      
        }
    }
}
