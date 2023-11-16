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
        private Mesh m_mesh;

        public List<int> triangles;
        public List<Vector3> verts;
    }
}
