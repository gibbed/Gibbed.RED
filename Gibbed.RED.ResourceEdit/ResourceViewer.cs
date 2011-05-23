/* Copyright (c) 2011 Rick (rick 'at' gibbed 'dot' us)
 * 
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 * 
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 * 
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Gibbed.RED.ResourceEdit
{
    public partial class ResourceViewer : Form
    {
        private string FilePath;
        private FileFormats.ResourceFile FileData;

        public ResourceViewer()
        {
            this.InitializeComponent();
            this.DoubleBuffered = true;
            this.hintLabel.Text = "";
        }

        public void LoadResource(string path)
        {
            this.Text += string.Format(": {0}", Path.GetFileName(path));

            this.FilePath = path;

            using (var input = File.OpenRead(path))
            {
                var rez = new FileFormats.ResourceFile();
                rez.Deserialize(input);
                this.FileData = rez;
            }

            this.BuildTree();
        }

        private void BuildTree()
        {
            this.entryTreeView.BeginUpdate();
            this.entryTreeView.Nodes.Clear();

            var root = new TreeNode(Path.GetFileName(this.FilePath));
            root.ImageKey = "RESOURCE";
            root.SelectedImageKey = "RESOURCE";

            var nodes =
                new Dictionary<FileFormats.Resource.Object, TreeNode>();
            var queue =
                new Queue<FileFormats.Resource.Object>(this.FileData.Objects);

            while (queue.Count > 0)
            {
                var obj = queue.Dequeue();

                var typeName = this.FileData.ReadString(obj.TypeNameIndex);
                var node = new TreeNode(typeName);

                if (this.entryTreeView.ImageList.Images.ContainsKey(typeName) == true)
                {
                    node.ImageKey = typeName;
                    node.SelectedImageKey = typeName;
                }
                else
                {
                    node.ImageKey = "";
                    node.SelectedImageKey = "";
                }

                node.Tag = obj;
                nodes.Add(obj, node);

                if (obj.Parent == null)
                {
                    root.Nodes.Add(node);
                }
                else
                {
                    nodes[obj.Parent].Nodes.Add(node);
                }

                if (obj.Children != null)
                {
                    foreach (var child in obj.Children)
                    {
                        if (queue.Contains(child) == false &&
                            nodes.ContainsKey(child) == false)
                        {
                            queue.Enqueue(child);
                        }
                    }
                }
            }

            this.entryTreeView.Nodes.Add(root);
            root.Expand();
            this.entryTreeView.EndUpdate();
        }

        private void OpenObject(TreeNode node)
        {
            var obj = node.Tag as FileFormats.Resource.Object;
            if (obj == null)
            {
                return;
            }

            var typeName = this.FileData.ReadString(obj.TypeNameIndex);
            if (typeName == "CBitmapTexture")
            {
                var viewer = new TextureViewer()
                {
                    MdiParent = this.MdiParent,
                };
                viewer.LoadFile(this.FileData, obj);
                viewer.Show();
            }
            else if (typeName == "CMesh")
            {
                var mesh = new FileFormats.Game.CMesh();
                obj.Data.Position = 0;
                mesh.Deserialize(this.FileData, obj.Data);
            }
            else if (typeName == "CEntityTemplate")
            {
                var template = new FileFormats.Game.CEntityTemplate();
                obj.Data.Position = 0;
                template.Deserialize(this.FileData, obj.Data);
            }
        }

        private void OnViewObject(object sender, EventArgs e)
        {
            this.OpenObject(this.entryTreeView.SelectedNode);
        }
    }
}
