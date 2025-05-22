using MoreEyes.Core;
using System;
using System.Linq;
using UnityEngine;
using static MoreEyes.EyeManagement.CustomEyeManager;

namespace MoreEyes.EyeManagement;

public class CustomIrisType
{
    public string Name = string.Empty;
    public string Path = string.Empty;
    public GameObject Prefab = null!;
    internal LoadedAsset MyBundle;
    public Sides AllowedPos = Sides.Both;
    public bool isVanilla = false;
    public bool inUse = false;

    //easier to go through lists in UnityExplorer
    public override string ToString() => Name;

    public void VanillaSetup()
    {
        Name = "None";
        isVanilla = true;
        AllIrisTypes.Add(this);
    }

    public void IrisSetup(LoadedAsset bundle, string name)
    {
        MyBundle = bundle;
        Path = name;

        Name = name[(name.LastIndexOf('/') + 1)..].Replace(".prefab", "");

        if (name.EndsWith("_right", StringComparison.OrdinalIgnoreCase))
        {
            AllowedPos = Sides.Right;
        }
        else if (name.EndsWith("_left", StringComparison.OrdinalIgnoreCase))
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
    public void MarkIrisUnused()
    {
        //might just use this lol
        inUse = false;

        if (IrisInUse.Contains(this))
        {
            IrisInUse.Remove(this);
        }
    }
}