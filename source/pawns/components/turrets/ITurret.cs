using Godot;
using System;

public interface ITurret
{
    // Returns True if the target is within the firiing arc of this turret.
    // E.G., will return false if the target is higher than the gun can 
    // elevate, or if the target is behind a turret with limited traverse.
    public bool TrySetTarget(Vector3 target);
}
