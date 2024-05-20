﻿using System;
using System.Collections.Generic;
using UnityEngine;
using static WooAsset.AssetsVersionCollection.VersionData;
using static WooAsset.AssetsVersionCollection;
using static WooAsset.ManifestData;

namespace WooAsset
{
    public static partial class AssetsInternal
    {

        private static BundleMap bundles;
        private static AssetMap assets;
        private static AssetsSetting setting;

        private static MixedAssetLife mixedlife;
        private static IAssetMode _defaultMode = new NormalAssetMode();
        public static IAssetMode mode { get; set; }
        private static string localSaveDir;

        public static void SetLocalSaveDir(string path)
        {
            localSaveDir = path;
            AssetsHelper.CreateDirectory(localSaveDir);
        }
        public static string GetLocalSaveDir() => localSaveDir;
        static AssetsInternal()
        {
            mode = _defaultMode;
            bundles = new BundleMap();
            assets = new AssetMap();
            SetLocalSaveDir(AssetsHelper.CombinePath(Application.persistentDataPath, "DLC"));
        }

        public static void AddAssetLife(IAssetLife life)
        {
            if (mixedlife == null)
            {
                mixedlife = new MixedAssetLife();
                bundles.SetListen(mixedlife);
                assets.SetListen(mixedlife);
            }
            mixedlife.AddLife(life);
        }
        public static void RemoveAssetLife(IAssetLife life)
        {
            if (mixedlife == null) return;
            mixedlife.RemoveAssetLife(life);
        }


    }
    partial class AssetsInternal
    {
        private static Bundle CreateBundle(BundleLoadArgs args) => mode.CreateBundle(args.bundleName, args);
        public static bool Initialized() => mode.Initialized();
        public static Operation InitAsync(string version, bool again, Func<VersionData, List<PackageData>> getPkgs) => mode.InitAsync(version, again, getPkgs);
        public static CheckBundleVersionOperation VersionCheck() => mode.VersionCheck();
        public static CopyStreamBundlesOperation CopyToSandBox() => mode.CopyToSandBox(AssetsHelper.streamBundleDirectory, localSaveDir);



        public static AssetType GetAssetType(string assetPath) => Initialized() ? GetAssetData(assetPath).type : AssetType.None;
        public static AssetData GetAssetData(string assetPath) => Initialized() ? mode.manifest.GetAssetData(assetPath) : null;
        public static BundleData GetBundleData(string bundleName) => Initialized() ? mode.manifest.GetBundleData(bundleName) : null;
        public static IReadOnlyList<string> GetAssetTags(string assetPath) => GetAssetData(assetPath)?.tags;



        public static IReadOnlyList<string> GetAllAssetPaths() => Initialized() ? mode.manifest.allPaths : null;
        public static IReadOnlyList<string> GetTagAssetPaths(string tag) => Initialized() ? mode.manifest.GetTagAssetPaths(tag) : null;
        public static IReadOnlyList<string> GetAllTags() => Initialized() ? mode.manifest.allTags : null;
        public static IReadOnlyList<string> GetAssetsByAssetName(string name, List<string> result) => Initialized() ? mode.manifest.GetAssetsByAssetName(name, result) : null;
        public static IReadOnlyList<string> GetAllAssetPaths(string bundleName) => Initialized() ? mode.manifest.GetAssets(bundleName) : null;


    }
    partial class AssetsInternal
    {
        private static int GetWebRequestTimeout() => setting.GetWebRequestTimeout();
        private static int GetWebRequestRetryCount() => setting.GetWebRequestRetryCount();
        private static string GetUrlFromBundleName(string bundleName) => setting.GetUrlByBundleName(AssetsHelper.buildTarget, bundleName);
        private static bool GetAutoUnloadBundle() => setting.GetAutoUnloadBundle();


        public static void SetAssetsSetting(AssetsSetting setting)
        {
            AssetsInternal.setting = setting;
            IAssetLife life = setting.GetAssetLife();
            if (life != null)
                AddAssetLife(life);
        }
        public static BundleDownloader DownLoadBundle(string bundleName) => new BundleDownloader(GetUrlFromBundleName(bundleName), GetWebRequestTimeout(), GetWebRequestRetryCount());
        public static FileDownloader DownLoadFile(string bundleName) => new FileDownloader(GetUrlFromBundleName(bundleName), GetBundleLocalPath(bundleName), GetWebRequestTimeout(), GetWebRequestRetryCount());
        public static Downloader DownloadVersion(string bundleName) => new Downloader(GetUrlFromBundleName(bundleName), GetWebRequestTimeout(), GetWebRequestRetryCount());
        public static Downloader CopyFile(string url, string path) => new FileDownloader(url, path, GetWebRequestTimeout(), GetWebRequestRetryCount());
        public static Downloader DownloadBytes(string url) => new Downloader(url, GetWebRequestTimeout(), GetWebRequestRetryCount());
        public static FileData.FileCompareType GetFileCheckType() => setting.GetFileCheckType();

        public static IAssetStreamEncrypt GetEncrypt() => setting.GetEncrypt();
        private static IAssetStreamEncrypt GetEncrypt(int enCode) => setting.GetEncrypt(enCode);
        public static bool GetSaveBundlesWhenPlaying() => setting.GetSaveBundlesWhenPlaying() && !GetBundleAwalysFromWebRequest();
        public static bool GetBundleAwalysFromWebRequest() => setting.GetBundleAwalysFromWebRequest();

        public static long GetLoadingMaxTimeSlice() => setting.GetLoadingMaxTimeSlice();
        public static bool NeedCopyStreamBundles() => setting.NeedCopyStreamBundles();
        public static string OverwriteBundlePath(string bundlePath) => setting.OverwriteBundlePath(bundlePath);

    }
    partial class AssetsInternal
    {
        public static string GetBundleLocalPath(string bundleName) => OverwriteBundlePath(AssetsHelper.CombinePath(localSaveDir, bundleName));
        public static void UnloadBundles() => bundles.UnloadBundles();

        public static Bundle LoadBundle(string bundleName, bool async)
        {
            var data = GetBundleData(bundleName);
            var bundle = bundles.Find(bundleName);
            Bundle[] dependence;
            if (bundle == null)
                dependence = new Bundle[data.dependence.Count];
            else
                dependence = bundle.dependence;
            for (int i = 0; i < data.dependence.Count; i++)
                dependence[i] = LoadBundle(data.dependence[i], async);
            return bundles.LoadAsync(new BundleLoadArgs(data, async, GetEncrypt(data.enCode), dependence));
        }

  

        public static Asset LoadFileAsset(string path, bool async) => assets.LoadAsync(AssetLoadArgs.FileArg(new ManifestData.AssetData()
        {
            bundleName = string.Empty,
            dps = null,
            path = path,
            tags = null,
            type = AssetType.None,
        }, async)) as Asset;

        public static Asset LoadResourceAsset(string path, bool async, AssetType type) => assets.LoadAsync(AssetLoadArgs.ResArg(new ManifestData.AssetData()
        {
            bundleName = string.Empty,
            dps = null,
            path = path,
            tags = null,
            type = type,
        }, async)) as Asset;
        public static AssetHandle LoadAsset(string path, bool async, Type type)
        {
            var data = GetAssetData(AssetsHelper.ToRegularPath(path));
            if (data == null)
            {
                AssetsHelper.LogError($"Not Found Asset: {path}"); 
                return null;
            }
            return assets.LoadAsync(AssetLoadArgs.NormalArg(data, async, type));
        }

        public static bool GetIsAssetLoaded(string assetPath) => assets.Find(assetPath) != null;



        public static void Release(string assetPath) => assets.Release(assetPath);



        public static IReadOnlyList<AssetHandle> GetLoadedAssets() => assets.GetAll();
        public static IReadOnlyList<Bundle> GetLoadedBundles() => bundles.GetAll();





    }
}
