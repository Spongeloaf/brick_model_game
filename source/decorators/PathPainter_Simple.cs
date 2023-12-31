using Godot;
using System;
using System.Drawing;

public partial class PathPainter_Simple : MeshInstance3D
{
  ImmediateMesh m_immediateMesh;

  public override void _Ready()
  {
    m_immediateMesh = new ImmediateMesh();
  }

  public void ClearPath()
  {
    m_immediateMesh.ClearSurfaces();
  }

  public void DrawPath(in Vector3[] points_global)
  {
    m_immediateMesh.ClearSurfaces();
    if (points_global.Length == 0)
      return;

    m_immediateMesh.SurfaceBegin(Mesh.PrimitiveType.LineStrip);

    foreach (Vector3 point in points_global)
    {
      Vector3 offset = point;
      //offset.Y += 1f;
      m_immediateMesh.SurfaceSetNormal(new Vector3(0, 0, 1));
      m_immediateMesh.SurfaceSetUV(new Vector2(0, 0));
      m_immediateMesh.SurfaceAddVertex(point);
      m_immediateMesh.SurfaceSetColor(Godot.Colors.White);
    }

    m_immediateMesh.SurfaceEnd();
    Mesh = m_immediateMesh;
  }
}
