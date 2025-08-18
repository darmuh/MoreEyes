﻿using MoreEyes.Core;
using System;
using System.Linq;
using UnityEngine;
using static MoreEyes.EyeManagement.CustomEyeManager;

namespace MoreEyes.EyeManagement;

internal class CustomPupilType
{
    internal string Name = string.Empty;
    internal string Path = string.Empty;
    internal GameObject Prefab = null!; //This object is used to instantiate the actual pupil object and is re-used by all players
    internal LoadedAsset MyBundle = null!;
    internal Sides AllowedPos = Sides.Both;
    internal bool isVanilla = false;
    internal bool inUse = false;

    //internal static List<string> UsedPupilNames = [];

    //easier to go through lists in UnityExplorer
    public override string ToString() => Name;

    internal CustomPupilType(LoadedAsset bundle, string name)
    {
        PupilSetup(bundle, name);
    }

    internal CustomPupilType(string name)
    {
        Name = name;
    }
    internal void PupilSetup(LoadedAsset bundle, string name)
    {
        MyBundle = bundle;
        Path = name;

        Name = name[(name.LastIndexOf('/') + 1)..].Replace(".prefab", "");

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

        MyBundle.LoadAssetGameObject(Path, out Prefab);
        if (Prefab == null)
            Plugin.logger.LogWarning($"PUPIL IS NULL FOR ASSETNAME - [ {Name} ]");
        Prefab.SetActive(false);
        UnityEngine.Object.DontDestroyOnLoad(Prefab);

        AllPupilTypes.Add(this);
        AllPupilTypes.Distinct();
        Plugin.Spam($"AllPupilTypes count - {AllPupilTypes.Count}");
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
        Path = eyeObject.name; //set for getting via rpc?
        Prefab = UnityEngine.Object.Instantiate(eyeObject);
        UnityEngine.Object.DontDestroyOnLoad(Prefab);
        Prefab.transform.SetParent(null);
        Prefab.SetActive(false);
        isVanilla = true;
    }
}
