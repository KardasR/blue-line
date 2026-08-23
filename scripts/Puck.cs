using System;
using Godot;

namespace BlueLine;

public partial class Puck : RigidBody3D
{
    #region Members

    private bool _isHeld;

    #endregion Members

    #region Properties

    /// <summary>
    /// A node that contains a collection of node3d's that represent different faceoff dots.
    /// </summary>
    public Node FaceoffLocations { get; set; }

    #endregion Properties

    #region Public Methods

    /// <summary>
    /// Shoot the puck in a specific direction.
    /// </summary>
    /// <param name="direction">Where to shoot the puck</param>
    /// <param name="force">How hard to shoot the puck</param>
    public void Shoot(Vector3 direction, float force)
    {
        if (!_isHeld)
            return;
        
        Reparent(GetTree().CurrentScene);

        Vector3 target = (
            direction - GlobalPosition
        ).Normalized();

        Freeze = false;

        LinearVelocity = target * force;

        _isHeld = false;
    }

    /// <summary>
    /// Freeze the puck and mark it as held so it doesn't fly off the players stick.
    /// </summary>
    /// <param name="grabPoint"></param>
    public void Grab(Node3D grabPoint)
    {
        if (_isHeld)
            return;
        
        _isHeld = true;

        ResetPuck();

        Freeze = true;
        GlobalPosition = grabPoint.GlobalPosition;
        
        Reparent(grabPoint);
    }

    /// <summary>
    /// Drops the puck at a given faceoff dot.
    /// </summary>
    /// <param name="faceoffDot">Where to drop the puck</param>
    /// <exception cref="InvalidOperationException">You must give a node object that is a collection of faceoff dots (area3d's)</exception>
    public void DropThePuck(FaceoffDot faceoffDot)
    {
        if (FaceoffLocations == null)
        {
            throw new InvalidOperationException("No faceoff location node was given. Cannot spawn puck.");
        }

        // Reset anything from prior use
        ResetPuck();
        
        Vector3 faceoffLocation = new();
        switch(faceoffDot)
        {
            case FaceoffDot.CenterIce:
                faceoffLocation = FaceoffLocations.GetNode<Node3D>("Center Ice").GlobalPosition;
                break;
            case FaceoffDot.HomeCenter:
                faceoffLocation = FaceoffLocations.GetNode<Node3D>("Home Center").GlobalPosition;
                break;
            case FaceoffDot.HomePenInzone:
                faceoffLocation = FaceoffLocations.GetNode<Node3D>("Home Pen Inzone").GlobalPosition;
                break;
            case FaceoffDot.HomeBenchInzone:
                faceoffLocation = FaceoffLocations.GetNode<Node3D>("Home Bench Inzone").GlobalPosition;
                break;
            case FaceoffDot.HomePenNeutral:
                faceoffLocation = FaceoffLocations.GetNode<Node3D>("Home Pen Neutral").GlobalPosition;
                break;
            case FaceoffDot.HomeBenchNeutral:
                faceoffLocation = FaceoffLocations.GetNode<Node3D>("Home Bench Neutral").GlobalPosition;
                break;
            case FaceoffDot.AwayCenter:
                faceoffLocation = FaceoffLocations.GetNode<Node3D>("Away Center").GlobalPosition;
                break;
            case FaceoffDot.AwayPenInzone:
                faceoffLocation = FaceoffLocations.GetNode<Node3D>("Away Pen Inzone").GlobalPosition;
                break;
            case FaceoffDot.AwayBenchInzone:
                faceoffLocation = FaceoffLocations.GetNode<Node3D>("Away Bench Inzone").GlobalPosition;
                break;
            case FaceoffDot.AwayPenNeutral:
                faceoffLocation = FaceoffLocations.GetNode<Node3D>("Away Pen Neutral").GlobalPosition;
                break;
            case FaceoffDot.AwayBenchNeutral:
                faceoffLocation = FaceoffLocations.GetNode<Node3D>("Away Bench Neutral").GlobalPosition;
                break;
        }
        
        GlobalPosition = faceoffLocation;
    }

    /// <summary>
    /// Halts the puck and makes sure that it's standing still.
    /// </summary>
    public void ResetPuck()
    {
        // Reset anything from prior use
        LinearVelocity = Vector3.Zero;
        AngularVelocity = Vector3.Zero;
        Rotation = Vector3.Zero;
    }

    #endregion Public Methods
}
