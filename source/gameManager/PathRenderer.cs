using Godot;
using System;

public partial class PathRenderer : MeshInstance3D
{
  ImmediateMesh m_immediateMesh;

  public override void _Ready()
  {
    m_immediateMesh = new ImmediateMesh();
  }

  public void DrawPath(in Vector3[] points)
  {
    if (points.Length == 0)
      return;

    m_immediateMesh.ClearSurfaces();
    m_immediateMesh.SurfaceBegin(Mesh.PrimitiveType.Lines);

    foreach (Vector3 point in points)
    {
      m_immediateMesh.SurfaceSetNormal(new Vector3(0, 0, 1));
      m_immediateMesh.SurfaceSetUV(new Vector2(0, 0));
      m_immediateMesh.SurfaceAddVertex(point);
    }

    m_immediateMesh.SurfaceEnd();

    Mesh = m_immediateMesh;
  }
}
