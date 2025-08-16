using BepInEx;
using HarmonyLib;
using MoreEyes.Core;
using System.Collections.Generic;
using UnityEngine;

namespace MoreEyes.EyeManagement;

internal class CustomEyeManager
{
    public static List<CustomPupilType> AllPupilTypes { get; internal set; } = [];
    public static List<CustomIrisType> AllIrisTypes { get; internal set; } = [];
    public static List<PatchedEyes> AllPatchedEyes { get; internal set; } = [];
    internal static List<PlayerEyeSelection> AllPlayerSelections = [];

    public static List<CustomPupilType> PupilsInUse = [];
    public static List<CustomIrisType> IrisInUse = [];

    public static bool isInitialized = false;

    public static CustomPupilType VanillaPupilRight { get; internal set; } = new("Standard Right");
    public static CustomPupilType VanillaPupilLeft { get; internal set; } = new("Standard Left");
    public static CustomIrisType VanillaIris;

    public enum Sides
    {
        Left,
        Right,
        Both
    }

    internal static void ClearLists()
    {
        AllPupilTypes.Clear();
        AllIrisTypes.Clear();
    }

    internal static void Init()
    {
        AllIrisTypes = [];
        AllPupilTypes = [VanillaPupilLeft, VanillaPupilRight];

        // Get all custom eye types
        // need to clear lists to not create duplicates
        // CustomEyeManager.ClearLists();
        // OR
        // We set an initialization bool and only load types once!
        // neither may be needed since we init at plugin awake
        if (!isInitialized)
        {
            Core.AssetManager.LoadedAssets.Do(asset =>
            {
                //This will go through any assets that have been registered with our mod
                GetAllTypes(asset);
            });

            Plugin.Spam("CustomEyeManager Initialized!");
            FileManager.ReadTextFile();
        }
    }

    //not used but may be useful at some point
    private static Transform RecursiveFindMatchingChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name.Contains(childName, System.StringComparison.InvariantCultureIgnoreCase))
            {
                return child;
            }
            else
            {
                Transform found = RecursiveFindMatchingChild(child, childName);
                if (found != null)
                {
                    return found;
                }
            }
        }
        return null;
    }

    internal static void GetAllTypes(LoadedAsset loadedAsset)
    {
        if(loadedAsset.Bundle == null)
        {
            Plugin.logger.LogWarning("Unable to get all types from loadedAsset, bundle is not loaded");
            return;
        }

        List<string> allAssets = [.. loadedAsset.Bundle.GetAllAssetNames()];
        List<string> irisNames = allAssets.FindAll(n => n.Contains("_iris_"));
        List<string> pupilNames = allAssets.FindAll(n => n.Contains("_pupil_"));

        // Make sure there's no weird names that dont lead to anything
        irisNames.RemoveAll(i => i.IsNullOrWhiteSpace());
        pupilNames.RemoveAll(p => p.IsNullOrWhiteSpace());

        // Load all custom pupils from asset into memory
        // Each pupil will load an object prefab that can be used to create a clone
        // since there's no fast/easy way to unload an individual asset, no need to hotload
        pupilNames.Do(n =>
        {
            CustomPupilType thisType = new(loadedAsset, n);
        });

        VanillaIris = new();
        VanillaIris.VanillaSetup();

        irisNames.Do(n =>
        {
            CustomIrisType thisType = new();
            thisType.IrisSetup(loadedAsset, n);
        });
    }
}
