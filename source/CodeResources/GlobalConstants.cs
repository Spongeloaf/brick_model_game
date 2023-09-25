//
// Project level constans like layer indexes and names, etc.
//

namespace Globals
{
  public static class LayerStrings
  {
    public const string Default = "Default";
    public const string Pawns = "Pawns";
  }

  public static class LayerInts
  {
    public const int Default = 0;
    public const int NoRayCast = 2;
    public const int Pawns = 6;
  }

  public static class UiStrings
  {
    public static string StartTurn = "Player {0} turn start";
    public static string Player_X = "Player {0}";
  }

  public static class Debugging
  {
    public static bool drawColliders = false;
  }

  public static class PawnDecoration
  {
    public static int OutlineWidth = 2;
  }

} // Globals