﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace WooAsset
{
    partial class AssetsEditorTool
    {
        public class DefaultAssetBuild : IAssetBuild
        {
            public override void Create(List<EditorAssetData> assets, List<EditorBundleData> result)
            {
                List<EditorAssetData> Shaders = assets.FindAll(x => x.type == AssetType.Shader || x.type == AssetType.ShaderVariant);
                assets.RemoveAll(x => x.type == AssetType.Shader || x.type == AssetType.ShaderVariant);
                EditorBundleTool.N2One(Shaders, result);

                List<EditorAssetData> Scenes = assets.FindAll(x => x.type == AssetType.Scene);
                assets.RemoveAll(x => x.type == AssetType.Scene);
                EditorBundleTool.One2One(Scenes, result);
                var tagAssets = assets.FindAll(x => x.tags != null && x.tags.Count != 0);
                assets.RemoveAll(x => tagAssets.Contains(x));
                var tags = tagAssets.SelectMany(x => x.tags).Distinct().ToList();
                tags.Sort();
                foreach (var tag in tags)
                {
                    List<EditorAssetData> find = tagAssets.FindAll(x => x.tags.Contains(tag));
                    tagAssets.RemoveAll(x => find.Contains(x));
                    EditorBundleTool.N2MBySize(find, result);
                }
                List<AssetType> _n2mSize = new List<AssetType>() {
                    AssetType.TextAsset
                };
                List<AssetType> _n2mSizeDir = new List<AssetType>() {
                     AssetType.Texture,
                     AssetType.Material,
                };
                List<AssetType> _one2one = new List<AssetType>() {
                    AssetType.Font,
                    AssetType.AudioClip,
                    AssetType.VideoClip,
                    AssetType.Prefab,
                    AssetType.Model,
                    AssetType.Animation,
                    AssetType.AnimationClip,
                    AssetType.AnimatorController,
                    AssetType.ScriptObject,
                };
                foreach (var item in _one2one)
                {
                    List<EditorAssetData> fits = assets.FindAll(x => x.type == item);
                    assets.RemoveAll(x => x.type == item);
                    EditorBundleTool.One2One(fits, result);
                }
                foreach (var item in _n2mSize)
                {
                    List<EditorAssetData> fits = assets.FindAll(x => x.type == item);
                    assets.RemoveAll(x => x.type == item);
                    EditorBundleTool.N2MBySize(fits, result);
                }
                foreach (var item in _n2mSizeDir)
                {
                    List<EditorAssetData> fits = assets.FindAll(x => x.type == item);
                    assets.RemoveAll(x => x.type == item);
                    EditorBundleTool.N2MBySizeAndDir(fits, result);
                }
                EditorBundleTool.N2MBySizeAndDir(assets, result);
            }

            public override string GetVersion(string settingVersion, AssetTaskContext context)
            {
                return DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
            }
      
        }



    }

}
