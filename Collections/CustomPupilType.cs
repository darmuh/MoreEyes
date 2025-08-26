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
internal class CustomPupilType
{
    internal string ModName = string.Empty;
    internal string Name = string.Empty;
    internal string AssetPath = string.Empty;
    internal string UID = string.Empty;
    internal string PairName = string.Empty;
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
    internal GameObject Prefab = null!; //This object is used to instantiate the actual pupil object and is re-used by all players
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
        string cleanedName = MenuUtils.CleanName(Name);
        PairName = char.ToUpper(cleanedName[0]) + cleanedName[1..] + " Pupil(s)";

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
            AllowedPos = PrefabSide.Left;
        }
        else
        {
            Name = "Standard";
            AllowedPos = PrefabSide.Right;
        }

        AddVanillaEye(original);
    }

    private void AddVanillaEye(GameObject eyeObject)
    {
        Prefab = UnityEngine.Object.Instantiate(eyeObject);
        UnityEngine.Object.DontDestroyOnLoad(Prefab);
        Prefab.transform.SetParent(null);
        Prefab.SetActive(false);
        isVanilla = true; //should already be set but set anyway
    }

    internal static List<CustomPupilType> GetListing(EyeSide side, PlayerEyeSelection selected)
    {
        List<CustomPupilType> listing = [];
        if (side == EyeSide.Left)
        {
            listing.AddRange(AllPupilTypes.FindAll(i => i.AllowedPos != PrefabSide.Right)); //don't get any right-only pupils
            listing.RemoveAll(p => !p.IsEnabled); //remove any that are disabled
            if(!listing.Contains(selected.pupilLeft)) //add current selection if it is not already in the list
                listing.Add(selected.pupilLeft);
            listing.DistinctBy(p => p.UID); //don't include duplicates
            return listing;
        }
        else
        {
            listing.AddRange(AllPupilTypes.FindAll(i => i.AllowedPos != PrefabSide.Left)); //don't get any left-only pupils
            listing.RemoveAll(p => !p.IsEnabled); //remove any that are disabled
            if (!listing.Contains(selected.pupilRight)) //add current selection if it is not already in the list
                listing.Add(selected.pupilRight);
            listing.DistinctBy(p => p.UID); //don't include duplicates
            return listing;
        }
    }

    //safely get index and always return 0 if indexof fails
    internal static int IndexOf(EyeSide side, List<CustomPupilType> options, PlayerEyeSelection selected)
    {
        Loggers.Debug($"Current Selections: Left - [{selected.pupilLeft.MenuName}] Right - [{selected.pupilRight.MenuName}]");
        int index;
        if (side == EyeSide.Left)
            index = options.IndexOf(selected.pupilLeft);
        else
            index = options.IndexOf(selected.pupilRight);

        //If menu has issues again, uncomment these logs to debug index issues
        //options.Do(o => Loggers.Debug(o.Name));
        //Loggers.Debug($"index - {index}");

        if (index < 0)
        {
            Loggers.Warning($"Failed to get index of current selections! Setting index to 0.\nSide:{side}\noptionCount:{options.Count}");
            index = 0;
        }

        Loggers.Debug($"Index of Iris from {side} {options.Count} is {index}");
        return index;
    }
}
