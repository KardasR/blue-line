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

    #endregion Properties

    #region Overrides

    public override void _Ready()
    {
        if (PuckScene == null)
        {
            throw new InvalidOperationException("Puck Scene was not given. Cannot spawn puck");
        }

        // create and add the puck to the scene
        // TODO: do this for the player too?
        Puck puck = PuckScene.Instantiate<Puck>();

        AddChild(puck);
        
        puck.FaceoffLocations = GetNode<Node>("Arena/Faceoff Dots");
        puck.DropThePuck(FaceoffDot.CenterIce);
    }

    #endregion Overrides

}
