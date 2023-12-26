namespace BrickModelGame.source.pawns
{
    public interface IDirectPawnController
    {
        // This function is used to issue commands directly to pawns, and is intended
        // to enable developers to quicklly test out new features or to enable the
        // game manager to issue commands to pawns without using a planner or executor.
        public void DoDirectAction(InputActions actions);
    }
}
