using BepInEx;
using MoreEyes.Core;
using MoreEyes.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace MoreEyes.Managers;
internal class EyesAssetManager
{
    internal static List<LoadedAsset> LoadedAssets = [];

    public static void InitBundles()
    {
        List<string> paths = [.. Directory.GetFiles(Path.Combine(Paths.BepInExRootPath, "plugins"), "*.eyes", SearchOption.AllDirectories)];

        foreach(string path in paths)
        {
            LoadedAsset existing = LoadedAssets.FirstOrDefault(a => a.isLoaded == true && a.FilePath == path);

            if (existing == null)
                existing = new(path);
            else
                Loggers.Warning($"The asset at path {path} has already been loaded!");
        }  
    }

    internal static void UnloadBundle(string bundlePath)
    {
        LoadedAsset existing = LoadedAssets.FirstOrDefault(a => a.isLoaded == true && a.FilePath == bundlePath);
        if(existing == null)
        {
            Loggers.Warning("There is no bundles that are currently loaded at that path!");
            return;
        }

        existing.UnloadBundle();
    }

    internal static void UnloadBundle(AssetBundle bundleRef)
    {
        if(bundleRef == null)
        {
            Loggers.Warning("Cannot unload a null bundleRef!");
            return;
        }
        
        LoadedAsset existing = LoadedAssets.FirstOrDefault(a => a.isLoaded == true && a.Bundle == bundleRef);

        if(existing == null)
        {
            Loggers.Warning("The provided bundleRef is not being detected as loaded.");
            return;
        }

        existing.UnloadBundle();
    }
}