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
    }
}
