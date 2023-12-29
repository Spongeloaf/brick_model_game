//
// Project level constans like layer indexes and names, etc.
//

using Godot;

public static class Globals
{
    public const float navigationCloseEnough = 0.5f;
    public const float projectileMaxDistance = 100f;   // This number is pulled out of my ass.
    public const bool drawRawNavigationPaths = true;    // Move this to project settings
    public const float m_pawnRotationSpeed = Mathf.Tau / 2;    // radians per second
    public const float minimumGimbalAngleDegrees = 5.0f;
    public static float minimumGimbalAngleRadians = Mathf.DegToRad(5.0f);

} // Globals