using System;
using Godot;

public partial class MainNode : Node
{
    #region Properties

    /// <summary>
    /// Scene that contains the puck we're going to use.
    /// </summary>
    [Export]
    public PackedScene PuckScene { get; set; }

    /// <summary>
    /// Where to drop the puck.
    /// </summary>
    [Export]
    public Node3D PuckSpawnPoint { get; set; }

    #endregion Properties

    #region Overrides

    public override void _Ready()
    {
        if (PuckScene == null)
        {
            throw new InvalidOperationException("Puck Scene was not given. Cannot spawn puck");
        }
        if (PuckSpawnPoint == null)
        {
            throw new InvalidOperationException("Puck Spawn Point was not given. Cannot spawn puck");
        }

        // create and add the puck to the scene
        // Puck puck = PuckScene.Instantiate<Puck>();

        // this.AddChild(puck);

        // puck.DropThePuck(PuckSpawnPoint.Position);
    }

    #endregion Overrides

}
