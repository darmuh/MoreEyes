using MoreEyes.Core;
using MoreEyes.SDK;
using MoreEyes.Utility;
using System;
using System.Linq;
using UnityEngine;
using static MoreEyes.Managers.CustomEyeManager;
using static MoreEyes.Utility.Enums;


namespace MoreEyes.Collections;
internal class CustomIrisType
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
            {
                var setting = ModConfig.ModNamesInMenu.Value;
                if (setting == ModInMenuDisplay.Never)
                    return MenuUtils.CleanName(Name);
                else if (setting == ModInMenuDisplay.Duplicates && !DuplicateNameExists)
                    return MenuUtils.CleanName(Name);
                else
                    return $"[{ModName}]" + $" {MenuUtils.CleanName(Name)}";
            }
        }
    }

    internal bool DuplicateNameExists
    {
        get
        {
            return AllPupilTypes.Count(i => MenuUtils.CleanName(i.Name) == MenuUtils.CleanName(Name) && i.AllowedPos == AllowedPos) > 1;
        }
    }

    internal GameObject Prefab = null!;
    internal LoadedAsset MyBundle = null!;
    internal PrefabSide AllowedPos = PrefabSide.Both;
    internal bool isVanilla = false;
    internal bool inUse = false;

    //easier to go through lists in UnityExplorer
    public override string ToString() => Name;

    internal void VanillaSetup()
    {
        Name = "None";
        AssetPath = "None";
        UID = "Vanilla";
        isVanilla = true;
        AllIrisTypes.Add(this);
    }

    internal void IrisSetup(LoadedAsset bundle, string assetName, MoreEyesMod mod)
    {
        MyBundle = bundle;
        AssetPath = assetName;
        ModName = mod.name;

        Name = assetName[(assetName.LastIndexOf('/') + 1)..].Replace(".prefab", "");
        UID = Name + "-" + mod.Name + "-" + mod.Author + "-" + mod.Version;

        if (Name.EndsWith("_right", StringComparison.OrdinalIgnoreCase))
        {
            AllowedPos = PrefabSide.Right;
        }
        else if (Name.EndsWith("_left", StringComparison.OrdinalIgnoreCase))
        {
            AllowedPos = PrefabSide.Left;
        }
        else
        {
            AllowedPos = PrefabSide.Both;
        }

        MyBundle.LoadAssetGameObject(AssetPath, out Prefab);
        if (Prefab == null)
            Loggers.Warning($"IRIS IS NULL FOR ASSETNAME - [ {Name} ]");
        Prefab.SetActive(false);
        UnityEngine.Object.DontDestroyOnLoad(Prefab);

        AllIrisTypes.Add(this);
        AllIrisTypes.Distinct();
    }
}