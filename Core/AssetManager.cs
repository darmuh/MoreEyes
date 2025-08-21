using BepInEx;
using MoreEyes.SDK;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace MoreEyes.Core;

internal class LoadedAsset
{
    public AssetBundle Bundle = null!;
    internal string FilePath = string.Empty;
    internal bool isLoaded = false;
    internal MoreEyesMod ModInfo = null!;

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
internal class AssetManager
{
    internal static List<LoadedAsset> LoadedAssets = [];

    public static void InitBundles()
    {
        List<string> paths = [.. Directory.GetFiles(Path.Combine(Paths.BepInExRootPath, "plugins"), "*.eyesbundle", SearchOption.AllDirectories)];

        foreach(string path in paths)
        {
            Plugin.Spam($"Loading eyesbundle at path: {path}");
            LoadedAsset existing = LoadedAssets.FirstOrDefault(a => a.isLoaded == true && a.FilePath == path);

            if (existing == null)
                existing = new(path);
            else
                Plugin.logger.LogWarning($"The asset at path {path} has already been loaded!");
        }  
    }

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
}