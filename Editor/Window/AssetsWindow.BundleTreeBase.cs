﻿using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

namespace WooAsset
{
    partial class AssetsWindow
    {
        private abstract class BundleTreeBase : TreeView
        {

            protected virtual List<BundleGroup> groups { get; private set; }
            protected IPing<BundleGroup> ping;
            protected void SetBundleGroups(List<BundleGroup> groups)
            {
                this.groups = groups;
                this.Reload();
            }

            public BundleTreeBase(TreeViewState state, IPing<BundleGroup> ping) : base(state)
            {
                var _base = new MultiColumnHeaderState.Column[]
                {
                    GetFirstColomn(),
                   TreeColumns.usageCount,
                    TreeColumns.depenceCount,
                TreeColumns.size,

                };

                this.multiColumnHeader = new MultiColumnHeader(new MultiColumnHeaderState(_base));
                this.multiColumnHeader.ResizeToFit();
                this.ping = ping;
                showAlternatingRowBackgrounds = true;
                Reload();
            }
            protected abstract MultiColumnHeaderState.Column GetFirstColomn();
            protected abstract void CreateRows(TreeViewItem root, IList<TreeViewItem> result);
            protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
            {
                var result = GetRows() ?? new List<TreeViewItem>();
                result.Clear();
                CreateRows(root, result);
                if (result.Count > 0)
                    SetupParentsAndChildrenFromDepths(root, result);
                return result;
            }
            protected TreeViewItem BuildBundle(int i, TreeViewItem root, IList<TreeViewItem> result)
            {
                var bundle = groups[i];
                var _item = new TreeViewItem()
                {
                    id = i,
                    depth = 0,
                    parent = root,
                    displayName = bundle.hash,
                };
                root.AddChild(_item);
                result.Add(_item);
                return _item;
            }
            protected override TreeViewItem BuildRoot()
            {
                return new TreeViewItem() { id = -10, depth = -1 };

            }
            protected override void RowGUI(RowGUIArgs args)
            {
                float indent = this.GetContentIndent(args.item);
                var rect1 = RectEx.Zoom(args.GetCellRect(0), TextAnchor.MiddleRight, new Vector2(-indent, 0));
                var rs = RectEx.VerticalSplit(rect1, 18);
                GUI.Label(rs[0], Textures.folder);
                EditorGUI.SelectableLabel(rs[1], args.label);

                BundleGroup group = cache.GetBundleGroupByBundleName(args.label);
                GUI.Label(args.GetCellRect(1), group.usage.Count.ToString());
                GUI.Label(args.GetCellRect(2), group.dependence.Count.ToString());
                GUI.Label(args.GetCellRect(3), GetSizeString(group.length));
            }
            protected override void DoubleClickedItem(int id)
            {
                var group = groups[id];
                ping.Ping(group);
            }

        }
    }
}