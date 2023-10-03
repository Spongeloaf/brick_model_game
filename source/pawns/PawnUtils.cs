using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class PawnUtils
{
  public static class Appearance
  {
    public static void SetHighlight(PawnController pawn)
    {
      if (pawn == null)
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

      Material material = mesh.MaterialOverride;
      material.NextPass = ResourceLoader.Load("res://assets/materials/highlightshader.tres") as Material;
    }

    public static void ClearHighlight(PawnController pawn)
    {
      if (pawn == null)
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

      Material material = mesh.MaterialOverride;
      material.NextPass = null;
    }
  }

  public static class Navigation
  {
    public static NavigationAgent3D GetNavAgent(PawnController pawn)
    {
      // TODO: We need a better way to do this. Add a function to PawnController???
      NavigationAgent3D agent = pawn.GetNode<NavigationAgent3D>("navAgent");

      if (agent == null)
        GD.PrintErr("GetNavAgent could not locate an agent for this pawn");

      return agent;
    }
  }
}