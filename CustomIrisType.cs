using System;
using System.Linq;
using UnityEngine;
using static MoreEyes.CustomEyeManager;

namespace MoreEyes;

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
        Name = "No Iris";
        isVanilla = true;
        AllIrisTypes.Add(this);
    }

    public void IrisSetup(LoadedAsset bundle, string name)
    {
        MyBundle = bundle;
        Path = name;
        //See AddCustomPupils comments
        Name = name[(name.LastIndexOf('/') + 1)..].Replace(".prefab", "");

        if (Name.EndsWith("_right"))
            AllowedPos = Sides.Right;
        else if (Name.EndsWith("_left"))
            AllowedPos = Sides.Left;

        MyBundle.LoadAssetGameObject(Path, out Prefab);
        if (Prefab == null)
            Plugin.logger.LogWarning($"IRIS IS NULL FOR ASSETNAME - [ {Name} ]");
        Prefab.SetActive(false);
        GameObject.DontDestroyOnLoad(Prefab);

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
            Plugin.logger.LogInfo($"{Name} was marked as unused."); // We can get rid of this logger in the future
        }
    }
}
