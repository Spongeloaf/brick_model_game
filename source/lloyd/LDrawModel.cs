using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Godot;

namespace LDraw
{
    public class LDrawModel
    {
        #region fields and properties

        private string _Name;
        private List<LDrawCommand> _Commands;
        private List<string> _SubModels;
        private static Dictionary<string, LDrawModel> _models = new Dictionary<string, LDrawModel>();

        public string Name
        {
            get { return _Name; }
        }
        #endregion
        /// FileFormatVersion 1.0.2;

        #region factory

        public static LDrawModel Create(string name, string path)
        {
            if (_models.ContainsKey(name)) return _models[name];
            LDrawModel model = new LDrawModel();
            model.ConstructModel(name, path);

            return model;
        }

        #endregion

        #region service methods

        private void ConstructModel(string name, string serialized)
        {
            _Name = name;
            _Commands = new List<LDrawCommand>();
            using (StringReader reader = new StringReader(serialized))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Regex regex = new Regex("[ ]{2,}", RegexOptions.None);
                    line = regex.Replace(line, " ").Trim();

                    if (!String.IsNullOrEmpty(line))
                    {
                        LDrawCommand command = LDrawCommand.DeserializeCommand(line, this);
                        if (command != null)
                            _Commands.Add(command);
                    }
                }
            }

            if (!_models.ContainsKey(name))
            {
                _models.Add(name, this);
            }
        }

        public Node3D CreateMeshGameObject(System.Numerics.Matrix4x4 trs, Material mat, Node parent, List<Node> createdNodes)
        {
            if (_Commands.Count == 0)
                return null;

            Node3D node = new Node3D();
            node.Name = _Name;
            createdNodes.Add(node);

            List<int> triangles = new List<int>();
            List<Vector3> verts = new List<Vector3>();

            for (int i = 0; i < _Commands.Count; i++)
            {
                LDrawSubFile sfCommand = _Commands[i] as LDrawSubFile;
                if (sfCommand == null)
                {
                    _Commands[i].PrepareMeshData(triangles, verts);
                }
                else
                {
                    sfCommand.GetModelNode(node, createdNodes);
                }
            }

            if (verts.Count > 0)
            {
                MeshInstance3D meshNode = new MeshInstance3D();
                node.AddChild(meshNode);
                createdNodes.Add(meshNode);
                meshNode.Mesh = PrepareMesh(verts, triangles, _Name);

                if (mat != null)
                    meshNode.Mesh.SurfaceSetMaterial(0, mat);
            }

            // GODOT PORT: ???
            node.Transform = TransformExtention.GetTransform(trs);

            if (parent != null)
                parent.AddChild(node);

            return node;
        }

        private Mesh PrepareMesh(List<Vector3> verts, List<int> triangles, string name)
        {

            Mesh mesh = LDrawConfig.Instance.GetMesh(name);
            if (mesh != null)
                return mesh;

            int frontVertsCount = verts.Count;

            //backface
            verts.AddRange(verts);
            int[] tris = new int[triangles.Count];
            triangles.CopyTo(tris);
            for (int i = 0; i < tris.Length; i += 3)
            {
                int temp = tris[i];
                tris[i] = tris[i + 1];
                tris[i + 1] = temp;
            }

            for (int i = 0; i < tris.Length; i++)
            {
                tris[i] = tris[i] + frontVertsCount;
            }
            triangles.AddRange(tris);

            SurfaceTool st = new SurfaceTool();
            st.Begin(Mesh.PrimitiveType.Triangles);
            foreach (Vector3 vert in verts)
            {
                st.AddVertex(vert);
            }

            foreach (int tri in tris)
                st.AddIndex(tri);

            st.GenerateNormals();
            mesh = st.Commit();
            mesh.ResourceName = _Name;
            //end backface

            //mesh.verts(verts)
            //mesh.SetTriangles(triangles, 0);

            //mesh.RecalculateNormals();
            //mesh.RecalculateBounds();
            //LDrawConfig.Instance.SaveMesh(mesh, mesh.ResourceName);
            return mesh;
        }

        #endregion

        private LDrawModel()
        {

        }
    }
}