using MoreEyes.Core;
using MoreEyes.Managers;
using MoreEyes.SDK;
using UnityEngine;

namespace MoreEyes.Collections
{
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
            EyesAssetManager.LoadedAssets.Add(this);
        }

        internal void LoadBundle()
        {
            if (isLoaded || Bundle != null)
                return;

            Bundle = AssetBundle.LoadFromFile(FilePath);
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
                Loggers.Error("Unable to loadasset, Bundle is null!");
                return;
            }

            gameObject = Bundle.LoadAsset<GameObject>(name);
        }
    }
}
