using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Unity physics moving objects refernce:
// https://dev.to/xavieroliver/moving-objects-in-unity-the-right-way-3jkb

/* Design Notes:

It may be wise to make all pawns kinematic. No reliance on dynamic physics.
This lets us do things like calculate trajectories based on dice rolls, and
with perfect control. 

This would force us to handle all collisions ourselves, which may be a good
thing. It could be problematic in edge cases, or handling explosions: If I
do a bad job handling them, the whole game feels off.

But if I do it, now I get to have thrown debris and minifigs that don't
automatically, cause cascading physics events. If a flying minifig hits 
another one, I can roll dice to see if he knocks the other down.
 
 */



public class ModelController : MonoBehaviour
{
  // Start is called before the first frame update
  void Start()
  {

  }

  void DoUpdate()
  {

  }
}
