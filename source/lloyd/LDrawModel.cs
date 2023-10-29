using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Godot;

namespace LDraw
{
  public class LDrawModel
  {
    /// FileFormatVersion 1.0.2;

    #region factory

    public static LDrawModel Create(string name, string path)
    {
      if (_models.ContainsKey(name)) return _models[name];
      LDrawModel model = new LDrawModel();
      model.Init(name, path);

      return model;
    }

    #endregion

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

    #region service methods

    private void Init(string name, string serialized)
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

    public Node3D CreateMeshGameObject(System.Numerics.Matrix4x4 trs, Material mat, Node parent)
    {
      if (_Commands.Count == 0) return null;
      Node3D node = new Node3D();
      node.Name = _Name;

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
          sfCommand.GetModelNode(node);
        }
      }

      if (mat != null)
      {
        // GODOT PORT: I'm not sure if "Mesh" is the correct type to search for.
        Godot.Collections.Array<Node> childMrs = node.FindChildren("*", "Mesh");
        foreach (Node child in childMrs)
        {
          Mesh mesh = child.GetNode<Mesh>(node.GetPath());
          if (mesh != null)
            mesh.SurfaceSetMaterial(0, mat);  // is surface 0 correct????
        }
      }

      if (verts.Count > 0)
      {
        MeshInstance3D meshNode = new MeshInstance3D();
        node.AddChild(meshNode);
        meshNode.Mesh = PrepareMesh(verts, triangles, _Name);

        if (mat != null)
          meshNode.Mesh.SurfaceSetMaterial(0, mat);
      }

      // GODOT PORT: ???
      node.Transform.ApplyLocalTRS(trs);

      parent.AddChild(node);
      return node;
    }

    private Mesh PrepareMesh(List<Vector3> verts, List<int> triangles, string name)
    {

      Mesh mesh = LDrawConfig.Instance.GetMesh(name);
      if (mesh != null)
        return mesh;

      mesh = new Mesh();
      mesh.ResourceName = _Name;
      int frontVertsCount = verts.Count;
      SurfaceTool st = new SurfaceTool();
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