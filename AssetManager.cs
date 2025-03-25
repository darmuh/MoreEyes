using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MoreEyes;

public class LoadedAsset
{
    public string FilePath = string.Empty;
    public AssetBundle Bundle = null!;
    public bool isLoaded = false;

    public LoadedAsset(string assetPath)
    {
        FilePath = assetPath;
        LoadBundle();
        AssetManager.LoadedAssets.Add(this);
    }

    internal void LoadBundle()
    {
        if (isLoaded || Bundle != null)
            return;

        Bundle = AssetBundle.LoadFromFile(FilePath);
        Plugin.Spam($"Bundle loaded from {FilePath}");
        isLoaded = true;
    }

    internal void UnloadBundle()
    {
        if (Bundle == null)
            return;

        Bundle.Unload(true);
        Bundle = null!;
        isLoaded = false;
    }


    internal void LoadAssetGameObject(string name, out GameObject gameObject)
    {
        gameObject = null!;
        if (Bundle == null)
        {
            Plugin.logger.LogError("Unable to loadasset, Bundle is null!");
            return;
        }

        gameObject = Bundle.LoadAsset<GameObject>(name);
    }
}
public class AssetManager
{
    internal static List<LoadedAsset> LoadedAssets = [];
    internal static LoadedAsset DefaultAssets = null!;

    public static LoadedAsset InitBundle(string bundlePath)
    {
        LoadedAsset existing = LoadedAssets.FirstOrDefault(a => a.isLoaded == true && a.FilePath == bundlePath);

        if(existing == null)
            existing = new(bundlePath);
        else
            Plugin.logger.LogWarning($"The asset at path {bundlePath} has already been loaded!");
            
        return existing;
    }

    //Unload via filepath string
    internal static void UnloadBundle(string bundlePath)
    {
        LoadedAsset existing = LoadedAssets.FirstOrDefault(a => a.isLoaded == true && a.FilePath == bundlePath);
        if(existing == null)
        {
            Plugin.logger.LogWarning("There is no bundles that are currently loaded at that path!");
            return;
        }

        existing.UnloadBundle();
    }

    //overload for AssetBundle reference
    internal static void UnloadBundle(AssetBundle bundleRef)
    {
        if(bundleRef == null)
        {
            Plugin.logger.LogWarning("Cannot unload a null bundleRef!");
            return;
        }
        
        LoadedAsset existing = LoadedAssets.FirstOrDefault(a => a.isLoaded == true && a.Bundle == bundleRef);

        if(existing == null)
        {
            Plugin.logger.LogWarning("The provided bundleRef is not being detected as loaded.");
            return;
        }

        existing.UnloadBundle();
    }


    //this method may be beneficial if we want to reload all irises/pupils
    internal static void UnloadAllBundles()
    {
        Plugin.Spam("Unloading ALL AssetBundles");
        LoadedAssets.DoIf(x => x.isLoaded && x.Bundle != null, x => x.UnloadBundle());
    }

    public static void HotReloadAsset()
    {
        // Unload unused assets from memory and reload them when necessary (this is not finished yet - go ham at it if you want to)
        UnloadAllBundles();


        /*
        foreach (var usedPupil in UsedPupilNames)
        {
            CustomPupilType used = new();
            used.AddCustomPupils(usedPupil);
            Plugin.logger.LogInfo($"Reloaded {usedPupil}");

            List<PlayerAvatar> allPlayers = SemiFunc.PlayerGetAll();
            allPlayers.ForEach(p => PlayerSpawnPatch.GetPlayerEyes(p)); // Reapply new eyes to all players
        }
        */
    }

}
