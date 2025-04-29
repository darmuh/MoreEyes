using BepInEx;
using HarmonyLib;
using MoreEyes.Core;
using System.Collections.Generic;
using UnityEngine;

namespace MoreEyes.EyeManagement;

public class CustomEyeManager
{
    public static List<CustomPupilType> AllPupilTypes = [];
    public static List<CustomIrisType> AllIrisTypes = [];
    internal static List<PatchedEyes> AllPatchedEyes = [];
    internal static List<PlayerEyeSelection> AllPlayerSelections = [];

    public static List<CustomPupilType> PupilsInUse = [];
    public static List<CustomIrisType> IrisInUse = [];

    public static List<GameObject> MarkedForDeletion = [];

    public static bool isInitialized = false;

    public static CustomPupilType VanillaPupilRight = new("Standard Right");
    public static CustomPupilType VanillaPupilLeft = new("Standard Left");
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
        AllIrisTypes.RemoveAll(t => t == null);
        AllPupilTypes.RemoveAll(t => t == null);

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

    internal static void CheckForVanillaPupils()
    {

        // Create vanilla pupils
        // This will create a copy of the object (prefab) for our class and disable it
        if (VanillaPupilLeft.Prefab == null)
        {
            VanillaPupilLeft.VanillaSetup(true, PlayerAvatar.instance.playerAvatarVisuals.playerEyes.pupilLeft.GetChild(0).gameObject);
        }
            
        if(VanillaPupilRight.Prefab == null)
        {
            VanillaPupilRight.VanillaSetup(false, PlayerAvatar.instance.playerAvatarVisuals.playerEyes.pupilRight.GetChild(0).gameObject);
        }
        
    }

    internal static void GetAllTypes(LoadedAsset loadedAsset)
    {
        if(loadedAsset.Bundle == null)
        {
            Plugin.logger.LogWarning("Unable to get all types from loadedAsset, bundle is not loaded");
            return;
        }

        // replaced hardcoded name list with one that will get from the bundle
        // also made it take a parameter so this method can be used with any LoadedAsset
        List<string> allAssets = [.. loadedAsset.Bundle.GetAllAssetNames()];
        List<string> irisNames = allAssets.FindAll(n => n.Contains("_iris_"));
        List<string> pupilNames = allAssets.FindAll(n => n.Contains("_pupil_"));

        // make sure there's no weird names that dont lead to anything
        irisNames.RemoveAll(i => i.IsNullOrWhiteSpace());
        pupilNames.RemoveAll(p => p.IsNullOrWhiteSpace());

        //Load all custom pupils from asset into memory
        //Each pupil will load an object prefab that can be used to create a clone
        //since there's no fast/easy way to unload an individual asset, no need to hotload
        pupilNames.Do(n =>
        {
            CustomPupilType thisType = new(loadedAsset, n);
        });

        //create blank class item for vanilla iris
        //this will have a null game object
        VanillaIris = new();
        VanillaIris.VanillaSetup();

        irisNames.Do(n =>
        {
            CustomIrisType thisType = new();
            thisType.IrisSetup(loadedAsset, n);
        });
    }

    public static void EmptyTrash()
    {
        MarkedForDeletion.DoIf(d => d != null, d => Object.Destroy(d));
        MarkedForDeletion.Clear();
        Plugin.Spam("Deleted Trash");
    }
}
