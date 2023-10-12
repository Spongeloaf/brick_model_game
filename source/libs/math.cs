using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public static class Math
{
  internal struct PathSegment
  {
    public PathSegment(Vector3 _p0, Vector3 _p1) { p0 = _p0; p1 = _p1; }
    public Vector3 p0;
    public Vector3 p1;
  };

  public static Vector3[] GloablizePoints(in Vector3[] points, in Node3D node)
  {
    Vector3[] result = new Vector3[points.Length];

    if (node == null)
      return result;

    // There must be a better way than a raw loop.
    for (int i = 0; i < points.Length; i++)
    {
      result[i] = node.ToGlobal(points[i]);
    }

    return result;
  }

  public static Vector3 LerpAlongPath(Vector3[] path, double startTime, double duration)
  {
    // Magically teleporting to zero is preferable to teleporting to Vector3.Infinity in this case.
    if (path.Length < 1)
      return Vector3.Zero;
    
    if (path.Length == 1)
      return path[0];

    // step 0: Get normalized time index
    double currentTime = Time.GetUnixTimeFromSystem();

    // Should be safe to cast ,we expect values between 0 and 1.
    float normalizedIndex = (float)InverseLerp(startTime, startTime + duration, currentTime);
    
    // Ensure that the pawn does not move past the endpoints if the time is wonky.
    if (normalizedIndex < 0.0f)
      normalizedIndex = 0.0f;

    if (normalizedIndex > 1.0f) 
      normalizedIndex = 1.0f;

    // Step 1: Get path lengths
    float pathLength = GetPathLength(path);
    float distanceTravelled = pathLength * normalizedIndex;
    Vector3 result = GetPointAlongPath(path, distanceTravelled);
    
    return Vector3.Zero;
  }

  private static Vector3 GetPointAlongPath(in Vector3[] path, float distanceAlongPath)
  {
    // Assumes sane values were passed in, i.e. distance is less than total path length
    // It is the responsibility of the caller to ensure this!
    float distanceMeasured = 0f;
    Vector3 result = path[0];

    PathSegment[] pathSegments = GetPathSegments(path);
    foreach (PathSegment segment in pathSegments) 
    {
      float segmentLength = segment.p0.DistanceTo(segment.p1);
      distanceMeasured += segmentLength;

      if (distanceMeasured < distanceAlongPath)
        continue;

      // At this point, we've found the segment we want
      float distanceAlongSegment = distanceAlongPath - (distanceMeasured - segmentLength);
      result = segment.p1 - segment.p0;
      result *= (distanceAlongSegment / segmentLength);
      break;
    }

    return result;
  }

  public static double InverseLerp(double start, double end, double target)
  {
    double totalLength = end - start;
    double targetlength = target - start;
    return targetlength / totalLength;
  }

  public static float GetPathLength(in Vector3[] path)
  {
    float result = 0f;
    int numPoints = path.Length;
    if (numPoints < 2)
      return result;

    int segments = numPoints - 1;
    for (int i = 0; i < segments; i++)
      result += path[i].DistanceTo(path[i + 1]);

    return result;
  }

  private static PathSegment[] GetPathSegments(in Vector3[] path)
  {
    int numPoints = path.Length;
    if (numPoints < 2)
      return new PathSegment[0];

    int segments = numPoints - 1;
    PathSegment[] result = new PathSegment[segments];
    for (int i = 0; i < segments; i++)
      result[i] = new PathSegment(path[i], path[i+1]);

    return result;
  }

  public static Vector3[] GetPathLimitedToLength(in Vector3[] path, in float length)
  {
    if (path.Length < 2)
      return path;

    if (GetPathLength(path) <= length)
      return path;

    // Assumes sane values were passed in, i.e. distance is less than total path length
    // It is the responsibility of the caller to ensure this!
    float distanceMeasured = 0f;
    List<Vector3> result = new List<Vector3>();

    PathSegment[] pathSegments = GetPathSegments(path);
    foreach (PathSegment segment in pathSegments)
    {
      float segmentLength = segment.p0.DistanceTo(segment.p1);
      distanceMeasured += segmentLength;

      if (distanceMeasured < length)
      {
        result.Add(segment.p0);
        continue;
      }

      // At this point, we've found the segment we want
      float distanceAlongSegment = length - (distanceMeasured - segmentLength);
      result.Add(segment.p0.MoveToward(segment.p1, distanceAlongSegment));
      break;
    }

    return result.ToArray();
  }
}
