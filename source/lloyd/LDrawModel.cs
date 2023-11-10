using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Godot;

namespace Lloyd
{
    public static class Constants
    {
        public static readonly int kMainColorCode = 16;
        public static readonly string kBFC = "BFC";
    }

    public enum VertexWinding
    {
        CCW,
        CW,
        Unknown,
    }


    public class LDrawModel
    {
        #region fields and properties

        private string _Name;
        private List<LDrawCommand> _Commands;
        private List<string> _SubModels;
        private static Dictionary<string, LDrawModel> _models = new Dictionary<string, LDrawModel>();
        public int mainColor;
        public VertexWinding m_winding = VertexWinding.Unknown;

        public string Name
        {
            get { return _Name; }
        }
        #endregion
        /// FileFormatVersion 1.0.2;

        #region factory
        public static LDrawModel Create(string name, string path, int mainColor = 1)
        {
            if (_models.ContainsKey(name)) return _models[name];
            LDrawModel model = new LDrawModel();
            model.mainColor = mainColor;
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
                        LDrawCommand command = LDrawCommand.DeserializeCommand(line, this, mainColor);
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
                if (sfCommand != null)
                {
                    sfCommand.GetModelNode(node, createdNodes);
                    continue;
                }

                LdrawMetaCommand metaCommand = _Commands[i] as LdrawMetaCommand;
                if (metaCommand != null)
                {
                    DoMetaCommand(metaCommand);
                    continue;
                }

                _Commands[i].PrepareMeshData(triangles, verts, m_winding);
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

        private void DoMetaCommand(LdrawMetaCommand metaComand)
        {
            switch (metaComand.m_command)
            {
                case MetaCommands.BFC:
                    m_winding = metaComand.m_winding;
                    break;
                default:
                    break;
            }
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

            mesh = st.Commit();
            mesh.ResourceName = _Name;
            return mesh;
        }

        #endregion

        private LDrawModel()
        {

        }
    }
}