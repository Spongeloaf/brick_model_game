using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GameManagerStates;
using Godot;

public static class PawnUtils
{
  public static PawnController GetPawnAtRaycastHit(in RaycastHit3D hit)
  {
    if (hit.collider == null)
      return null;

    Type objType = hit.collider.GetType();
    if (objType == typeof(PawnController))
      return (PawnController)hit.collider;

    if (objType != typeof(CollisionShape3D))
      return null;

    CollisionShape3D collider = (CollisionShape3D)hit.collider;
    return collider.GetParent<PawnController>();
  }

  public static class Appearance
  {
    private static PackedScene m_labelTemplate = ResourceLoader.Load<PackedScene>("res://source/pawns/decorators/TextLabel.tscn");

    public static void SetHighlightGreen(PawnController pawn)
    {
      Material mat = ResourceLoader.Load("res://assets/materials/pawnMaterialHighlighted_Green.tres") as Material;
      SetMaterial(pawn, mat);
    }

    public static void SetHighlightRed(PawnController pawn)
    {
      Material mat = ResourceLoader.Load("res://assets/materials/pawnMaterialHighlighted_Red.tres") as Material;
      SetMaterial(pawn, mat);
    }

    public static void ClearHighlight(PawnController pawn)
    {
      Material mat = ResourceLoader.Load("res://assets/materials/pawnMaterial.tres") as Material;
      SetMaterial(pawn, mat);
    }

    private static void SetMaterial(PawnController pawn, Material material)
    {
      if (pawn == null || material == null)
        return;

      MeshInstance3D mesh = null;
      try
      {
        mesh = pawn.GetNode<MeshInstance3D>("collider/mesh");
        if (mesh == null)
          return;
      }
      catch
      {
        GD.PrintErr("Exception raised while looking for child node inside SetHighlight()!");
        return;
      }

      mesh.MaterialOverride = material;
    }
  
    public static void SetOverheadText(PawnController pawn, in string text)
    {
      if (pawn == null) 
        return;

      Node labelNode = m_labelTemplate.Instantiate();
      pawn.AddChild(labelNode);
      GodotObject obj = (GodotObject)labelNode;
      obj.Set("text", text);
    }
  }

  public static class Navigation
  {
    // Uses the Navigation server to get a nav path for the pawn's nav agent to the destination
    // The "_global" postfix indicates that the argument must be a global position, not a local one.
    public static Vector3[] GetNavigationPath(PawnController pawn, in Vector3 target_global)
    {
      if (pawn == null)
        return new Vector3[0];

      NavigationAgent3D agent = pawn.GetNavigationAgent3D();
      if (agent == null)
        return new Vector3[0];

      return NavigationServer3D.MapGetPath(pawn.GetWorld3D().NavigationMap, pawn.GlobalPosition, target_global, true);
    }
  }
}
