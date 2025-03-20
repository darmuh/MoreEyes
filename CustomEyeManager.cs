using BepInEx;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace MoreEyes
{
    public class CustomEyeManager
    {
        //Collection of all possible types
        public static List<CustomPupilType> AllPupilTypes = [];
        public static List<CustomIrisType> AllIrisTypes = [];
        internal static List<PatchedEyes> AllPatchedEyes = [];

        //Our list of things in Use, if needed
        public static List<CustomPupilType> PupilsInUse = [];
        public static List<CustomIrisType> IrisInUse = [];

        //Trashcan
        public static List<GameObject> MarkedForDeletion = [];

        //Initialized
        public static bool isInitialized = false;

        internal static void ClearLists()
        {
            AllPupilTypes.Clear();
            AllIrisTypes.Clear();
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

            // Create vanilla pupils
            // This will create a copy of the object for our class and disable it
            CustomPupilType vanilla_left = new("vanilla_left");
            vanilla_left.AddVanillaEye(PlayerAvatar.instance.playerAvatarVisuals.playerEyes.pupilLeft.GetChild(0).gameObject);

            CustomPupilType vanilla_right = new("vanilla_right");
            vanilla_right.AddVanillaEye(PlayerAvatar.instance.playerAvatarVisuals.playerEyes.pupilRight.GetChild(0).gameObject);

            pupilNames.Do(n =>
            {
                CustomPupilType thisType = new(n);
                thisType.AddCustomPupils();
            });

            //create blank class item for vanilla iris
            //this will have a null game object
            CustomIrisType noIris = new("vanilla");
            AllIrisTypes.Add(noIris);

            irisNames.Do(n =>
            {
                CustomIrisType thisType = new(n);
                thisType.AddCustomIris();
            });
        }

        public static void EmptyTrash()
        {
            MarkedForDeletion.DoIf(d => d != null, d => GameObject.Destroy(d));
            MarkedForDeletion.Clear();
            Plugin.Spam("Deleted Trash");
        }
    }
}
