//
// Project level constans like layer indexes and names, etc.
//

using Godot;

public static class Globals
{
  public static float navigationCloseEnough = 0.5f;
  public static float projectileMaxDistance = 100f;   // This number is pulled out of my ass.
  public static bool drawRawNavigationPaths = true;
  public static float m_pawnRotationSpeed = Mathf.Tau / 2;    // radians per second

} // Globals