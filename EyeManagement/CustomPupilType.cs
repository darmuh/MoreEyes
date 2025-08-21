using MoreEyes.Core;
using MoreEyes.Menus;
using MoreEyes.SDK;
using System;
using System.Linq;
using UnityEngine;
using static MoreEyes.EyeManagement.CustomEyeManager;

namespace MoreEyes.EyeManagement;
internal class CustomPupilType
{
    internal string ModName = string.Empty;
    internal string Name = string.Empty;
    internal string AssetPath = string.Empty;
    internal string UID = string.Empty;
    internal string MenuName
    {
        get
        {
            if (string.IsNullOrEmpty(ModName))
                return MenuUtils.CleanName(Name);
            else
                return $"[{ModName}]" + $" {MenuUtils.CleanName(Name)}"; 
        }
    }
    internal GameObject Prefab = null!; //This object is used to instantiate the actual pupil object and is re-used by all players
    internal LoadedAsset MyBundle = null!;
    internal Sides AllowedPos = Sides.Both;
    internal bool isVanilla = false;
    internal bool inUse = false;

    //easier to go through lists in UnityExplorer
    public override string ToString() => Name;

    internal CustomPupilType(LoadedAsset bundle, string assetName, MoreEyesMod mod)
    {
        PupilSetup(bundle, assetName, mod);
    }

    internal CustomPupilType(string name)
    {
        Name = name;
    }
    internal void PupilSetup(LoadedAsset bundle, string assetName, MoreEyesMod mod)
    {
        MyBundle = bundle;
        AssetPath = assetName;
        ModName = mod.name;

        Name = assetName[(assetName.LastIndexOf('/') + 1)..].Replace(".prefab", "");

        UID = Name + "-" + mod.Name + "-" + mod.Author + "-" + mod.Version;

        if (Name.EndsWith("_right", StringComparison.OrdinalIgnoreCase))
        {
            AllowedPos = Sides.Right;
        }
        else if (Name.EndsWith("_left", StringComparison.OrdinalIgnoreCase))
        {
            AllowedPos = Sides.Left;
        }
        else
        {
            AllowedPos = Sides.Both;
        }

        MyBundle.LoadAssetGameObject(AssetPath, out Prefab);
        if (Prefab == null)
            Loggers.Warning($"PUPIL IS NULL FOR ASSETNAME - [ {Name} ]");
        Prefab.SetActive(false);
        UnityEngine.Object.DontDestroyOnLoad(Prefab);

        AllPupilTypes.Add(this);
        AllPupilTypes.Distinct();
    }

    internal void VanillaSetup(bool isLeft, GameObject original)
    {
        if(isLeft)
        {
            Name = "Standard";
            AllowedPos = Sides.Left;
        }
        else
        {
            Name = "Standard";
            AllowedPos = Sides.Right;
        }

        AddVanillaEye(original);
    }

    private void AddVanillaEye(GameObject eyeObject)
    {
        Prefab = UnityEngine.Object.Instantiate(eyeObject);
        UnityEngine.Object.DontDestroyOnLoad(Prefab);
        Prefab.transform.SetParent(null);
        Prefab.SetActive(false);
        isVanilla = true;
    }
}
