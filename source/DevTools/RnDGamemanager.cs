using BrickModelGame.source.pawns;
using Godot;
using System.Collections.Generic;

namespace BrickModelGame.source.DevTools
{
    // This game manmager is intended to be used for testing and debugging.
    // It enables the developer to issue commands directly to pawns without
    // setting up executors or planners.
    //
    // Here's what you need:
    // 1. A scene with a player interface
    // 2. Add this manager as a sibling to the plaher interface
    // 3. Add pawns to the scene as children of this manager
    // 4. Add some funtionality to the pawns 'IDirectPawnController.DoDirectAction()`
    //    function.
    //
    // You will have the standard set of inputs that a game manager
    // would have, so complex interactions like selecting pawns won't work,
    // and every direct child of this manager will be issued commands simultaneously.
    //  
    // If you want to add pawns and don't want them to recvieve commands from this
    // manager, add a non-pawn interface node between this manager and those pawns.
    public partial class RnDGamemanager : Node3D, IGameManager
    {
        List<IDirectPawnController> m_Pawns = new();

        public override void _Ready()
        {
            foreach (Node3D node in GetChildren())
            {
                if (node is IDirectPawnController pawn)
                {
                    m_Pawns.Add(pawn);
                }
            }
        }

        public void DoUpdate(InputActions inputs)
        {
            foreach (IDirectPawnController pawn in m_Pawns)
            {
                pawn.DoDirectAction(inputs);
            }
        }
    }
}
