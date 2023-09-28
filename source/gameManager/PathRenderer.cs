using Godot;
using System;

public partial class PathRenderer : MeshInstance3D
{
  ImmediateMesh m_immediateMesh;

  public override void _Ready()
  {
    m_immediateMesh = new ImmediateMesh();
  }

  public void DrawPath_Global(in Vector3[] points)
  {
    if (points.Length == 0)
      return;

    m_immediateMesh.ClearSurfaces();
    m_immediateMesh.SurfaceBegin(Mesh.PrimitiveType.LineStrip);

    foreach (Vector3 point in points)
    {
      Vector3 offset = point;
      //offset.Y += 1f;
      m_immediateMesh.SurfaceSetNormal(new Vector3(0, 0, 1));
      m_immediateMesh.SurfaceSetUV(new Vector2(0, 0));
      m_immediateMesh.SurfaceAddVertex(point);
    }

    m_immediateMesh.SurfaceEnd();
    Mesh = m_immediateMesh;
  }
}
