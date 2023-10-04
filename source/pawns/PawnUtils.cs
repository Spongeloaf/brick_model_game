using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GameManagerStates;
using Godot;

public static class PawnUtils
{
  public static class Appearance
  {
    public static void SetHighlight(PawnController pawn)
    {
      Material mat = ResourceLoader.Load("res://assets/materials/pawnMaterialHighlighted.tres") as Material;
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
