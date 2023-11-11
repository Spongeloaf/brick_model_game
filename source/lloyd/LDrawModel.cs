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
        private bool m_bfcInvertNext = false;
        public bool m_inverted = false; // True when the subfile is inverted by a BFC INVERTNEXT command in the parent file.

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

            foreach (LDrawCommand cmd in _Commands)
                ProcessCommand(cmd, node, createdNodes, triangles, verts);

            if (verts.Count > 0)
            {
                MeshInstance3D meshNode = new MeshInstance3D();
                node.AddChild(meshNode);
                createdNodes.Add(meshNode);
                meshNode.Mesh = PrepareMesh(verts, triangles, _Name);

                if (mat != null)
                    meshNode.Mesh.SurfaceSetMaterial(0, mat);
            }

            node.Transform = TransformExtention.GetTransform(trs);
            if (parent != null)
                parent.AddChild(node);

            return node;
        }

        private void ProcessCommand(LDrawCommand cmd, Node3D node, List<Node> createdNodes, List<int> triangles, List<Vector3> verts)
        {
            // This simplifies the logic in the loop. If we break early
            // we don't have to remember to set InvertNext to false.
            bool invertNext = m_bfcInvertNext;
            if (m_bfcInvertNext)
                m_bfcInvertNext = false;

            LDrawSubFile sfCommand = cmd as LDrawSubFile;
            if (sfCommand != null)
            {
                sfCommand.GetModelNode(node, createdNodes, invertNext);
                return;
            }

            LdrawMetaCommand metaCommand = cmd as LdrawMetaCommand;
            if (metaCommand != null)
            {
                DoMetaCommand(metaCommand);
                return;
            }
            cmd.m_winding = GetWindingForThisCommand(invertNext, m_winding);
            cmd.PrepareMeshData(triangles, verts);
        }

        private void DoMetaCommand(LdrawMetaCommand metaComand)
        {
            switch (metaComand.m_command)
            {
                case MetaCommands.BFC:
                    m_bfcInvertNext = metaComand.m_invertNext;

                    // Don't lose the previous winding order if this command is just INVERTNEXT
                    if (m_bfcInvertNext)
                        break;

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

            st.GenerateNormals();
            mesh = st.Commit();
            mesh.ResourceName = _Name;
            return mesh;
        }

        private VertexWinding GetWindingForThisCommand(bool invert, VertexWinding winding)
        {
            if (winding == VertexWinding.Unknown)
                return VertexWinding.Unknown;

            // This should only be true when BFC INVERTNEXT is used in the parent file.
            if (m_inverted)
                invert = !invert;

            VertexWinding result = winding;
            if (invert)
            {
                if (winding == VertexWinding.CCW)
                    result = VertexWinding.CW;
                else
                    result = VertexWinding.CCW;
            }
            return result;
        }
        
        
        #endregion



        private LDrawModel()
        {

        }
    }
}