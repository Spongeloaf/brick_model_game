using Godot;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;


namespace Ldraw
{
    using LdrColor = System.Int32;
    using MeshLayer = System.Int32;

    public class MeshManager
    {
        private Dictionary <MeshLayer, LdrColor> m_meshColors = new Dictionary<int, LdrColor>();
        private Dictionary<string, int> m_billOfMaterials = new Dictionary<string, int>();
        private SurfaceTool m_surfaceTool = new SurfaceTool();
        private ArrayMesh m_ArrayMesh = new ArrayMesh();
        public readonly Transform3D m_ScaleToGameCoords;
        public readonly Transform3D m_RotateToGameOrientation;
        public Mesh m_mesh = new Mesh();

        // The Godot surface tool uses a normel per vertex, rather than per face.
        // However, we don't care (yet?) about per-vertex normals, so just use the
        // whole face normal for each vertex and call it a day.
        private Dictionary<LdrColor, Surface> m_surfaces = new();
        
        private class Surface
        {
            public Surface() {}
            public List<FaceData> faces = new();
            public List<int> triangleIndices = new();
        }
        
        private struct FaceData
        {
            public FaceData() {}
            public Vector3 vertex = Vector3.Zero;
            public Vector3 normal = Vector3.Zero;
            public LdrColor color = 0;
        }

        public MeshManager()
        {
            m_ScaleToGameCoords = Transform3D.Identity;
            m_RotateToGameOrientation = Transform3D.Identity;
            m_RotateToGameOrientation.Basis = m_RotateToGameOrientation.Basis.Rotated(Vector3.Left, Mathf.Pi);
            m_ScaleToGameCoords.Basis = m_ScaleToGameCoords.Basis.Scaled(new Vector3(0.01f, 0.01f, 0.01f));
        }

        public void AddTriangle(in Command cmd)
        {
            Vector3[] vertices = Parsing.DeserializeTriangle(cmd.commandString);

            if (vertices.Length != 3)
            {
                GD.PrintErr("Triangle command must have 3 vertices");
                return;
            }

            VertexWinding winding = cmd.metadata.winding;
            if (cmd.transform.Basis.Determinant() < 0)
                winding = Parsing.GetInvertedWinding(winding);

            if (winding == VertexWinding.CCW)
                FlipTriangleWinding(ref vertices);

            AddTriangleMeshData(in vertices, in cmd.transform, in cmd.color, winding);

            if (winding == VertexWinding.Unknown)
            {
                // If the winding is unknown, duplicate the data and flip the winding
                FlipTriangleWinding(ref vertices);
                AddTriangleMeshData(in vertices, in cmd.transform, in cmd.color, winding);
            }
        }

        private void AddTriangleMeshData(in Vector3[] inputVerts, in Transform3D tfm, in LdrColor color, VertexWinding winding)
        {
            Surface surface = GetSurface(in color);
            var vertLen = surface.faces.Count;
            for (int i = 0; i < 3; i++)
                surface.triangleIndices.Add(vertLen + i);

            Vector3 normal = CalculateNormal(inputVerts, winding);
            for (int i = 0; i < inputVerts.Length; i++)
            {
                FaceData faceData = new FaceData();
                faceData.vertex = m_RotateToGameOrientation * m_ScaleToGameCoords * tfm * inputVerts[i];
                faceData.normal = normal;
                faceData.color = color;
                surface.faces.Add(faceData);
            }
        }

        private static void FlipTriangleWinding(ref Vector3[] verts)
        {
            Vector3 tmp = verts[0];
            verts[0] = verts[1];
            verts[1] = tmp;
        }

        private void BuildMesh()
        {
            foreach (Surface surface in m_surfaces.Values)
                BuildSurface(in surface);
            
            m_surfaces.Clear();
        }

        private void BuildSurface(in Surface surface)
        {
            if (surface == null)
                return;

            m_surfaceTool.Begin(Mesh.PrimitiveType.Triangles);
            foreach (FaceData face in surface.faces)
            {
                // Normal MUST be set before vertex!
                // Also, if you ever wanted vertex color, set it before
                // the vertex value as well.
                m_surfaceTool.SetNormal(face.normal);
                m_surfaceTool.AddVertex(face.vertex);
            }

            foreach (int tri in surface.triangleIndices)
                m_surfaceTool.AddIndex(tri);

            m_surfaceTool.GenerateNormals();
            m_surfaceTool.GenerateTangents();
            m_ArrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, m_surfaceTool.CommitToArrays());
            
            int surfaceIndex = m_ArrayMesh.GetSurfaceCount() - 1;
            SetSurfaceColor(surfaceIndex, surface.faces[0].color);

            m_surfaceTool.Clear();
        }

        private void SetSurfaceColor(in int surfaceIndex, in LdrColor color)
        {
            Material material = MaterialManager.GetColoredMaterial(color);
            m_ArrayMesh.SurfaceSetMaterial(surfaceIndex, material);
        }

        public MeshInstance3D GetMeshInstance()
        {
            BuildMesh();
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
            //
            // It's possible that the original code was wrong. I suspect that
            // it would be correct to check normals for both triangles separtely
            // in the case that not all four points are planar.
            //
            // Maybe if I ever want to support BFC, I'd need to actually use the
            // 4th point for one of the two normals.
            //var nA = Vector3.Cross(v[1] - v[0], v[2] - v[0]);
            //var nB = Vector3.Cross(v[1] - v[0], v[2] - v[0]);

            Vector3[] v = vertices;
            Vector3 tmp = v[1] - v[0];
            Vector3 nA = tmp.Cross(v[2] - v[0]);
            Vector3 nB = tmp.Cross(v[2] - v[0]);

            Surface surface = GetSurface(in cmd.color);
            int vertLen = surface.faces.Count;

            VertexWinding winding = cmd.metadata.winding;
            if (cmd.transform.Basis.Determinant() < 0)
                winding = Parsing.GetInvertedWinding(winding);

            Vector3 normal = CalculateNormal(in v, winding);

            // If the winding is unknown, we assume CCW on the first pass and then do another CW pass.
            if (winding == VertexWinding.CW || winding == VertexWinding.Unknown)
            {
                surface.triangleIndices.AddRange(new[]
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
                surface.triangleIndices.AddRange(new[]
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
                FaceData faceData = new FaceData();
                faceData.vertex = m_RotateToGameOrientation * m_ScaleToGameCoords * cmd.transform * v[indexes[i]];
                faceData.normal = normal;
                faceData.color = cmd.color;
                surface.faces.Add(faceData);
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

        private Surface GetSurface(in LdrColor color)
        {
            Surface surface;
            m_surfaces.TryGetValue(color, out surface);
            if (surface == null)
            {
                surface = new();
                m_surfaces.Add(color, surface);
            }

            return surface;
        }

        private Vector3 CalculateNormal(in Vector3[] verts, VertexWinding winding)
        {
            Vector3 u = verts[1] - verts[0];
            Vector3 v = verts[2] - verts[0];
            return (winding == VertexWinding.CW ? 1 : -1) * u.Cross(v).Normalized();
        }
    }
}
