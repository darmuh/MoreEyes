using BepInEx.Configuration;
using MoreEyes.Core;
using MoreEyes.SDK;
using MoreEyes.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
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
                ModInMenuDisplay setting = ModConfig.ModNamesInMenu.Value;
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
    internal ConfigEntry<bool> ConfigToggle { get; set; } = null!;
    internal bool IsEnabled
    {
        get
        {
            if (ConfigToggle == null || isVanilla)
                return true;

            return ConfigToggle.Value;
        }
    }

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

    internal static List<CustomIrisType> GetListing(EyeSide side, PlayerEyeSelection selected)
    {
        List<CustomIrisType> listing = [];
        if (side == EyeSide.Left)
        {
            listing.AddRange(AllIrisTypes.FindAll(i => i.AllowedPos != PrefabSide.Right)); //don't get any right-only
            listing.RemoveAll(i => !i.IsEnabled); //remove any that are disabled
            if (!listing.Contains(selected.irisLeft)) //add current selection if it is not already in the list
                listing.Add(selected.irisLeft);
            listing.DistinctBy(i => i.UID); //don't include duplicates
            return listing;
        }
        else
        {
            listing.AddRange(AllIrisTypes.FindAll(i => i.AllowedPos != PrefabSide.Left)); //don't get any left-only
            listing.RemoveAll(i => !i.IsEnabled); //remove any that are disabled
            if (!listing.Contains(selected.irisRight)) //add current selection if it is not already in the list
                listing.Add(selected.irisRight);
            listing.DistinctBy(i => i.UID); //don't include duplicates
            return listing;
        }
    }

    //safely get index and always return 0 if indexof fails
    internal static int IndexOf(EyeSide side, List<CustomIrisType> options, PlayerEyeSelection selected)
    {
        int index;
        if (side == EyeSide.Left)
            index = options.IndexOf(selected.irisLeft);
        else
            index = options.IndexOf(selected.irisRight);

        if (index < 0)
        {
            Loggers.Warning($"Failed to get index of current selections! Setting index to 0.\nSide:{side}\noptionCount:{options.Count}");
            index = 0;
        }

        Loggers.Debug($"Index of Iris from {side} {options.Count} is {index}");
        return index;
    }
}