using Godot;

namespace BrickModelGame.source.pawns.components.turrets
{
    public partial class ElevatedWeapon : Area3D
    {
        public void SetTarget(Vector3 targetGlobalPos)
        {
            float distance = GlobalPosition.DistanceTo(targetGlobalPos);
            float elevation = Mathf.RadToDeg(Mathf.Asin((targetGlobalPos.Y - GlobalPosition.Y) / distance));
            RotationDegrees = new Vector3(-elevation, 0, 0);
        }
    }
}
