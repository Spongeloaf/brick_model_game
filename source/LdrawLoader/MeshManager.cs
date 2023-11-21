using Godot;
using System.Collections.Generic;

namespace Ldraw
{
    using LdrColor = System.Int32;
    using MeshLayer = System.Int32;

    public class MeshManager
    {
        private Dictionary<LdrColor, MeshLayer> m_colorsInMesh = new Dictionary<LdrColor, MeshLayer>();
        private Dictionary<string, int> m_billOfMaterials = new Dictionary<string, int>();
        private SurfaceTool m_surfaceTool = new SurfaceTool();
        public Mesh m_mesh = new Mesh();

        public List<int> triangles = new List<int>();
        public List<Vector3> verts = new List<Vector3>();

        public void AddTriangle(in Command cmd)
        {
            Vector3[] vertices = Parsing.DeserializeTriangle(cmd.commandString);

            if (vertices.Length != 3)
            {
                GD.PrintErr("Triangle command must have 3 vertices");
                return;
            }

            if (cmd.metadata.winding == VertexWinding.CCW)
                FlipTriangleWinding(ref vertices);

            AddTriangleMeshData(in vertices);

            if (cmd.metadata.winding == VertexWinding.Unknown)
            {
                // If the winding is unknown, duplicate the data and flip the winding
                FlipTriangleWinding(ref vertices);
                AddTriangleMeshData(in vertices);
            }

            TransformPoints(in cmd.transform);
            BuildMesh(in cmd);
        }

        private void TransformPoints(in Transform3D transform)
        {
            for (int i = 0; i < verts.Count; i++)
            {
                // While you're at it, you probably want to fix the parsing function
                // so it uses Godot native transforms instead of Matrix4x4.
                verts[i] = ???
            }
        }

        private void AddTriangleMeshData(in Vector3[] inputVerts)
        {
            var vertLen = verts.Count;
            for (int i = 0; i < 3; i++)
            {
                triangles.Add(vertLen + i);
            }

            for (int i = 0; i < inputVerts.Length; i++)
            {
                verts.Add(inputVerts[i]);
            }
        }

        private static void FlipTriangleWinding(ref Vector3[] verts)
        {
            Vector3 tmp = verts[0];
            verts[0] = verts[1];
            verts[1] = tmp;
        }

        private void BuildMesh(in Command cmd)
        {
            m_surfaceTool.Begin(Mesh.PrimitiveType.Triangles);
            foreach (Vector3 vert in verts)
            {
                m_surfaceTool.AddVertex(vert);
            }

            foreach (int tri in triangles)
                m_surfaceTool.AddIndex(tri);

            m_surfaceTool.GenerateNormals();
            m_mesh = m_surfaceTool.Commit();
        }

        public MeshInstance3D GetMeshInstance()
        {
            MeshInstance3D meshInstance = new MeshInstance3D();
            meshInstance.Mesh = m_mesh;
            return meshInstance;
        }
    }
}
