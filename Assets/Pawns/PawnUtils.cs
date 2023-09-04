using Globals;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PawnUtils
{
  public static void SetHighlight(List<GameObject> objects, UnityEngine.Color color)
  {
    if (objects == null)
      return;

    foreach (GameObject obj in objects)
      SetHighlight(obj, color);
  }

  public static void SetHighlight(GameObject pawn, UnityEngine.Color color)
  {
    if (pawn == null) return;

    QuickOutline outline = pawn.GetComponentInChildren<QuickOutline>();
    if (outline == null)
      return;

    outline.OutlineColor = color;
    outline.enabled = true;
    outline.OutlineWidth = PawnDecoration.OutlineWidth;
    
    // We need this to support creating and drawing on a single frame.
    outline.ForceUpdate();
  }

  public static void ClearHighlight(GameObject pawn)
  {
    if (pawn == null) return;

    QuickOutline outline = pawn.GetComponentInChildren<QuickOutline>();
    if (outline == null)
      return;

    outline.enabled = false;
  }
}
