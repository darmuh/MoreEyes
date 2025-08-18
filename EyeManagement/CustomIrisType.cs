using MoreEyes.Core;
using System;
using System.Linq;
using UnityEngine;
using static MoreEyes.EyeManagement.CustomEyeManager;

namespace MoreEyes.EyeManagement;

internal class CustomIrisType
{
    internal string Name = string.Empty;
    internal string Path = string.Empty;
    internal GameObject Prefab = null!; //This object is used to instantiate the actual iris object and is re-used by all players
    internal LoadedAsset MyBundle = null!;
    internal Sides AllowedPos = Sides.Both;
    internal bool isVanilla = false;
    internal bool inUse = false;

    //easier to go through lists in UnityExplorer
    public override string ToString() => Name;

    internal void VanillaSetup()
    {
        Name = "None";
        Path = "None";
        isVanilla = true;
        AllIrisTypes.Add(this);
    }

    internal void IrisSetup(LoadedAsset bundle, string name)
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
            Plugin.logger.LogWarning($"IRIS IS NULL FOR ASSETNAME - [ {Name} ]");
        Prefab.SetActive(false);
        UnityEngine.Object.DontDestroyOnLoad(Prefab);

        AllIrisTypes.Add(this);
        AllIrisTypes.Distinct();
        Plugin.Spam($"AllIrisTypes count - {AllIrisTypes.Count}");
    }

    //note sure if we are gonna use this to be honest
    internal void MarkIrisUnused()
    {
        //might just use this lol
        inUse = false;

        if (IrisInUse.Contains(this))
        {
            IrisInUse.Remove(this);
        }
    }
}