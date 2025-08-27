﻿using BepInEx;
using HarmonyLib;
using MoreEyes.Collections;
using MoreEyes.Components;
using MoreEyes.Core;
using MoreEyes.SDK;
using MoreEyes.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MoreEyes.Managers;
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
    public static bool VanillaPupilsExist => VanillaPupilLeft?.Prefab != null && VanillaPupilRight?.Prefab != null;

    internal static void Init()
    {
        AllIrisTypes = [];
        //Add vanilla pupils here to avoid any duplicates
        VanillaPupilLeft.isVanilla = true;
        VanillaPupilRight.isVanilla = true;
        AllPupilTypes = [VanillaPupilLeft, VanillaPupilRight];

        if (!isInitialized)
        {
            //Moved this out of the loop so only one vanilla iris is set
            VanillaIris = new();
            VanillaIris.VanillaSetup();
            EyesAssetManager.LoadedAssets.Do(asset =>
            {
                //This will go through any assets that have been registered with MoreEyesMod scriptable objects
                GetAllTypes(asset);
            });

            ModConfig.GenerateConfigItems();
            FileManager.ReadTextFile();
        }
    }

    internal static void GetAllTypes(LoadedAsset loadedAsset)
    {
        if(loadedAsset.Bundle == null)
        {
            Loggers.Warning("Unable to get all types from loadedAsset, bundle is not loaded");
            return;
        }

        //Get Mod Info from loaded asset
        var scriptableObjects = loadedAsset.Bundle.LoadAllAssets<ScriptableObject>();
        var modInfo = scriptableObjects.FirstOrDefault(p => p is MoreEyesMod);
        if (modInfo == null)
        {
            Loggers.Error($"Mod info is null for {loadedAsset.Bundle.name}!");
            return;
        }
            
        loadedAsset.ModInfo = modInfo as MoreEyesMod;
        List<GameObject> prefabsLoaded = [];

        List<string> allAssets = [.. loadedAsset.Bundle.GetAllAssetNames()];
        List<string> irisNames = allAssets.FindAll(n => n.Contains("_iris_"));
        List<string> pupilNames = allAssets.FindAll(n => n.Contains("_pupil_"));

        // Make sure there's no weird names that dont lead to anything
        irisNames.RemoveAll(i => i.IsNullOrWhiteSpace());
        pupilNames.RemoveAll(p => p.IsNullOrWhiteSpace());

        // Load all custom pupils from asset into memory
        pupilNames.Do(n =>
        {
            CustomPupilType thisType = new(loadedAsset, n, loadedAsset.ModInfo);
            prefabsLoaded.Add(thisType.Prefab);
        });

        irisNames.Do(n =>
        {
            CustomIrisType thisType = new();
            thisType.IrisSetup(loadedAsset, n, loadedAsset.ModInfo);
            prefabsLoaded.Add(thisType.Prefab);
        });

        //Replace prefabs list with what has been loaded
        loadedAsset.ModInfo.SetPrefabs(prefabsLoaded);
    }
}
