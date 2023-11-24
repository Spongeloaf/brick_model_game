using Godot;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;


namespace Ldraw
{
    using LdrColor = System.Int32;
    using MeshLayer = System.Int32;

    public class MeshManager
    {
        private Dictionary<LdrColor, MeshLayer> m_colorsInMesh = new Dictionary<LdrColor, MeshLayer>();
        private Dictionary<string, int> m_billOfMaterials = new Dictionary<string, int>();
        private SurfaceTool m_surfaceTool = new SurfaceTool();
        private ArrayMesh m_ArrayMesh = new ArrayMesh();
        
        public Mesh m_mesh = new Mesh();
        public List<int> triangles = new List<int>();
        public List<Vector3> verts = new List<Vector3>();
        public List<Vector3> normals = new List<Vector3>();

        private VertexWinding GetInvertedWinding(VertexWinding winding)
        {
            if (winding == VertexWinding.CCW)
                return VertexWinding.CW;
            else if (winding == VertexWinding.CW)
                return VertexWinding.CCW;
            else
                return VertexWinding.Unknown;
        }

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

            AddTriangleMeshData(in vertices, in cmd.transform);

            if (cmd.metadata.winding == VertexWinding.Unknown)
            {
                // If the winding is unknown, duplicate the data and flip the winding
                FlipTriangleWinding(ref vertices);
                AddTriangleMeshData(in vertices, in cmd.transform);
            }
        }

        private void AddTriangleMeshData(in Vector3[] inputVerts, in Transform3D tfm)
        {
            var vertLen = verts.Count;
            for (int i = 0; i < 3; i++)
                triangles.Add(vertLen + i);

            for (int i = 0; i < inputVerts.Length; i++)
                verts.Add(tfm * inputVerts[i]);
        }

        private static void FlipTriangleWinding(ref Vector3[] verts)
        {
            Vector3 tmp = verts[0];
            verts[0] = verts[1];
            verts[1] = tmp;
        }

        public void BuildMesh()
        {
            m_surfaceTool.Begin(Mesh.PrimitiveType.Triangles);
            foreach (Vector3 vert in verts)
            {
                m_surfaceTool.AddVertex(vert);
            }

            foreach (int tri in triangles)
                m_surfaceTool.AddIndex(tri);

            m_surfaceTool.GenerateNormals();
            m_surfaceTool.GenerateTangents();
            m_ArrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, m_surfaceTool.CommitToArrays());
            m_surfaceTool.Clear();
            verts.Clear();
            triangles.Clear();
        }

        public MeshInstance3D GetMeshInstance()
        {
            MeshInstance3D meshInstance = new MeshInstance3D();
            meshInstance.Mesh = m_ArrayMesh;
            return meshInstance;
        }

        public void AddQuad(in Command cmd)
        {
            Vector3[] vertices = Parsing.DeserializeQuad(cmd.commandString);
            if (vertices.Length != 4)
            {
                GD.PrintErr("Quad command must have 4 vertices");
                return;
            }

            // Why do we need nA and nB, when they're the same value?
            // Was there someting else I was missing? Vertex winding or similar?
            //var nA = Vector3.Cross(v[1] - v[0], v[2] - v[0]);
            //var nB = Vector3.Cross(v[1] - v[0], v[2] - v[0]);

            Vector3[] v = vertices;
            Vector3 tmp = v[1] - v[0];
            Vector3 nA = tmp.Cross(v[2] - v[0]);
            Vector3 nB = tmp.Cross(v[2] - v[0]);

            int vertLen = verts.Count;

            // If the winding is unknown, we assume CCW on the first pass and then do another CW pass.
            if (cmd.metadata.winding == VertexWinding.CW || cmd.metadata.winding == VertexWinding.Unknown)
            {
                triangles.AddRange(new[]
                {
                vertLen,
                vertLen + 1,
                vertLen + 2,
                vertLen + 2,
                vertLen + 3,
                vertLen
                });
            }
            else
            {
                triangles.AddRange(new[]
                {
                vertLen + 1,
                vertLen,
                vertLen + 2,
                vertLen + 2,
                vertLen,
                vertLen + 3,
                });
            }

            int[] indexes = nA.Dot(nB) < 0 ? new int[] { 0, 1, 3, 2 } : new int[] { 0, 1, 2, 3 };
            for (int i = 0; i < indexes.Length; i++)
            {
                verts.Add(cmd.transform * v[indexes[i]]);
            }

            // This is dirty......
            if (cmd.metadata.winding == VertexWinding.Unknown)
            {
                // We are relying on the if statment above to ensure
                // we wind CCW on the first pass, if the winding is unknown.
                Command cmd2 = cmd;
                cmd2.metadata.winding = VertexWinding.CCW;
                AddQuad(in cmd2);
            }
        }

        public Vector3 CalculateNormal(in Vector3[] verts, VertexWinding winding)
        {
            Vector3 u = verts[1] - verts[0];
            Vector3 v = verts[2] - verts[0];
            return (winding == VertexWinding.CW ? 1 : -1) * u.Cross(v).Normalized();
        }
    }
}
