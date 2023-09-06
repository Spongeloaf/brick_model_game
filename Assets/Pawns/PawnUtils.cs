using Globals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public static class PawnUtils
{
  public static class Appearance
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

  public static class Navigation
  {
    public static NavMeshAgent GetNavAgent(GameObject pawn)
    {
      NavMeshAgent agent = pawn.GetComponent<NavMeshAgent>();

      if (agent == null)
        Debug.LogError("GetNavAgent could not locate an agent for this pawn");

      return agent;
    }

    public static float GetPathLength(in Vector3[] path)
    {
      float result = 0f;
      int numPoints = path.Length;
      if (numPoints < 2)
        return result;

      int segments = numPoints - 1;
      for (int i = 0; i < segments; i++)
        result += Vector3.Distance(path[i], path[i + 1]);

      return result;
    }

    public static void LerpObjectAlongPath(PawnController pawn,
                                            in Vector3[] path,
                                            float startTime,
                                            float endTime)
    {
      throw new NotImplementedException();

      // The second for loop is wonky: It does a poor job of figuring out
      // which segment we are in.
      //
      // TO FIX:
      // 
      // * Remove the time calls, make it parameter
      // * make a test scene with a path, and hook up the time to a slider or something
      // * Now you can run the bithc like a unit test, kinda.
      // * Then consider actually writing a unit test for this.

      if (pawn == null || path == null)
        return;

      if (path.Length < 2)
        return;

      if (startTime > endTime)
      {
        float temp = startTime;
        startTime = endTime;
        endTime = temp;
      }

      int segmentCount = path.Length - 1;
      float totalDist = 0f;
      List<float> segmentLengths = new List<float>();

      for (int i = 0; i < segmentCount; i++)
      {
        float dist = Vector3.Distance(path[i], path[i + 1]);
        segmentLengths.Add(dist);
        totalDist += dist;
      }

      float lerpIndex = Mathf.InverseLerp(startTime, endTime, Time.time);
      float progress = 0f;
      float targetSegmentLength = 0f;
      float distanceFromStart = totalDist * lerpIndex;
      int targetSegmentIndex = 0;

      // This is magical bullshit. We find which segment the lerp should be in right now
      // by iterating each segment, and checking if the lerp is in it currently. If yes,
      // we store some values we can use to figure how far into that segment we are.
      for (/* n/a */ ; targetSegmentIndex < segmentLengths.Count; targetSegmentIndex++)
      {
        if (distanceFromStart > segmentLengths[targetSegmentIndex] + progress)
        {
          progress += segmentLengths[targetSegmentIndex];
          continue;
        }
          targetSegmentLength = segmentLengths[targetSegmentIndex];
          break;
      }

      // we've found our target segment
      distanceFromStart -= (progress);

      // Now distanceFromStart contains the actual distance we've travelled in the target segment
      float segmentLerpIndex = Mathf.InverseLerp(progress, progress + targetSegmentLength, distanceFromStart);

      Vector3 newPosition = new Vector3();
      if (targetSegmentIndex < path.Count() - 1)
      {
        // Now find the points for this segment
        Vector3 segmentStart = path[targetSegmentIndex];
        Vector3 segmentEnd = path[targetSegmentIndex + 1];

        // segmentLerpIndex should now contain the lerp value of the target segment!
        newPosition = Vector3.Lerp(segmentStart, segmentEnd, segmentLerpIndex);
      }
      else
      {
        newPosition = path.Last();
      }

      pawn.transform.position = newPosition;
    }
  }
}
