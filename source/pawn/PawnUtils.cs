using Globals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class PawnUtils
{
  public static class Appearance
  {
    //public static void SetHighlight(List<GameObject> objects, UnityEngine.Color color)
    //{
    //  if (objects == null)
    //    return;

    //  foreach (GameObject obj in objects)
    //    SetHighlight(obj, color);
    //}

    //public static void SetHighlight(GameObject pawn, UnityEngine.Color color)
    //{
    //  if (pawn == null) return;

    //  QuickOutline outline = pawn.GetComponentInChildren<QuickOutline>();
    //  if (outline == null)
    //    return;

    //  outline.OutlineColor = color;
    //  outline.enabled = true;
    //  outline.OutlineWidth = PawnDecoration.OutlineWidth;

    //  // We need this to support creating and drawing on a single frame.
    //  outline.ForceUpdate();
    //}

    //public static void ClearHighlight(GameObject pawn)
    //{
    //  if (pawn == null) return;

    //  QuickOutline outline = pawn.GetComponentInChildren<QuickOutline>();
    //  if (outline == null)
    //    return;

    //  outline.enabled = false;
    //}
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
