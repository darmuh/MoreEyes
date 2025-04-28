using MoreEyes.Core;
using System;
using System.Linq;
using UnityEngine;
using static MoreEyes.EyeManagement.CustomEyeManager;

namespace MoreEyes.EyeManagement;

public class CustomPupilType
{
    public string Name = string.Empty;
    public string Path = string.Empty;
    public GameObject Prefab = null!;
    public LoadedAsset MyBundle = null!;
    public Sides AllowedPos = Sides.Both;
    public bool isVanilla = false;
    public bool inUse = false;

    //internal static List<string> UsedPupilNames = [];

    //easier to go through lists in UnityExplorer
    public override string ToString() => Name;

    public CustomPupilType(LoadedAsset bundle, string name)
    {
        PupilSetup(bundle, name);
    }

    public CustomPupilType(string name)
    {
        Name = name;
    }

    public void PupilSetup(LoadedAsset bundle, string name)
    {
        MyBundle = bundle;
        Path = name;

        // the method to get the asset names dynamically includes the whole path
        // this will replace the string we've assigned without the path at the front
        // it also replaces ".prefab" at the end with nothing
        Name = name[(name.LastIndexOf('/') + 1)..].Replace(".prefab", "");

        if (Name.EndsWith("_right"))
            AllowedPos = Sides.Right;
        else if (Name.EndsWith("_left"))
            AllowedPos = Sides.Left;

        MyBundle.LoadAssetGameObject(Path, out Prefab);
        if (Prefab == null)
            Plugin.logger.LogWarning($"PUPIL IS NULL FOR ASSETNAME - [ {Name} ]");
        Prefab.SetActive(false);
        UnityEngine.Object.DontDestroyOnLoad(Prefab);

        AllPupilTypes.Add(this);
        AllPupilTypes.Distinct();
        Plugin.Spam($"AllPupilTypes count - {AllPupilTypes.Count}");
    }

    public void VanillaSetup(bool isLeft, GameObject original)
    {
        if(isLeft)
        {
            Name = "Standard Left";
            AllowedPos = Sides.Left;
        }
        else
        {
            Name = "Standard Right";
            AllowedPos = Sides.Right;
        }

        AddVanillaEye(original);

        AllPupilTypes.Add(this);
        AllPupilTypes.Distinct();
    }

    internal void AddVanillaEye(GameObject eyeObject)
    {
        Prefab = UnityEngine.Object.Instantiate(eyeObject);
        UnityEngine.Object.DontDestroyOnLoad(Prefab);
        Prefab.transform.SetParent(null);
        Prefab.SetActive(false);
        isVanilla = true;
    }

    public void MarkPupilUnused()
    {
        inUse = false;

        if (PupilsInUse.Contains(this))
        {
            PupilsInUse.Remove(this);
            Plugin.logger.LogInfo($"{Name} was marked as unused."); // We can get rid of this logger in the future
        }      
    }
}
