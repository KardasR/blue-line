using Godot;

public partial class Puck : RigidBody3D
{
    #region Public Methods

    public void DropThePuck(Vector3 spawnPoint)
    {
        Position = spawnPoint;
    }

    #endregion Public Methods
}
