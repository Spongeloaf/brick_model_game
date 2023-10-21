using Godot;
using System;

public static class SkillCheck
{
  public struct Paramters
  {
    public int useRating;
    public int useModifiers;
    public int skillDie;
  }

  private static RandomNumberGenerator m_dieRoller = new RandomNumberGenerator();

  // All skill checks can fail on a roll of 1, so the use rating MUST reflect that.
  // Also, since skill check passes if the roll is equal or higher, the hard floor
  // for use ratings is 2.
  private const int kUseRatingFloor = 2;

  public static bool Do(Paramters parameters)
  {
    SanitizeParameters(ref parameters);
    int roll = m_dieRoller.RandiRange(1, parameters.skillDie);

    // There is ALWAYS a chance to fail
    if (roll == 1)
      return false;

    // There is ALWAYS a chance to succeed
    if (roll == parameters.skillDie)
      return true;

    int modifiedRoll = roll + parameters.useModifiers;
    return modifiedRoll >= parameters.useRating;
  }

  private static void SanitizeParameters(ref Paramters pms)
  {
    // You cannot have fewer than two sides on a die.
    if (pms.skillDie < 2)
      pms.skillDie = 2;

    // The use rating can have modifiers pile up on it like crazy.
    // we must ensure that there is always at least a 1 in X chance
    // of the check suceeding AND failing. We cannot allow any cases
    // where the skilll check is mathematically guaranteed to pass
    // or fail.
    if (pms.useRating < kUseRatingFloor)
      pms.useRating = kUseRatingFloor;

    int modiifiedUseRating = (int)pms.useRating + pms.useModifiers;
    if (modiifiedUseRating < kUseRatingFloor)
    {
      int difference = (int)pms.useRating - pms.useModifiers;
      pms.useModifiers -= - difference + kUseRatingFloor;
    }
  }

  public static void RunTests() 
  {
    TestSkillCheckSanatization();
  }

  private static void TestSkillCheckSanatization()
  {
    // This is just for debugger testing.
    Paramters pm = new Paramters();
    pm.skillDie = 0;
    pm.useModifiers = 0;
    pm.skillDie = 0;
    SanitizeParameters(ref pm);
    
    pm.skillDie = 6;
    pm.useModifiers = 0;
    pm.useRating = 4;
    SanitizeParameters(ref pm);

    pm.skillDie = 6;
    pm.useModifiers = -99;
    pm.useRating = 4;
    SanitizeParameters(ref pm);

    pm.skillDie = 6;
    pm.useModifiers = 99;
    pm.useRating = 4;
    SanitizeParameters(ref pm);
    return; 
  }
}

