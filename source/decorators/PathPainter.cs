using Godot;
using System;

public partial class PathPainter : Node3D
{
  // Examples of C# -> GDScript
  // myGDScriptNode.Call("print_array", argument);
  // myGDScriptNode.Get("my_field");
  // myGDScriptNode.Set("my_field", "FOO");
  GodotObject m_pathObject;
  PathPainter_Simple m_Debugger;

  public override void _Ready()
  {
    //GDScript pathScript = (GDScript)GD.Load("res://addons/vizpath/visualized_path.gd");
    m_pathObject = (GodotObject)GetChild(0);

    m_Debugger = new PathPainter_Simple();
    AddChild((Node)m_Debugger);
  }

  public void ClearPath()
  {
    m_pathObject.Call("DrawPathWithHead", new Vector3[0]);

    if (m_Debugger != null && Globals.drawRawNavigationPaths)
      m_Debugger.ClearPath();
  }

  public void DrawPath(in Vector3[] points_global, uint lengthLimit = 0, bool printNodes = false)
  {
    m_pathObject.Call("DrawPathWithHead", points_global);

    if (m_Debugger != null && Globals.drawRawNavigationPaths)
      m_Debugger.DrawPath(points_global);

    if (printNodes)
      m_pathObject.Call("PrintDebugInfo");
  }
}
