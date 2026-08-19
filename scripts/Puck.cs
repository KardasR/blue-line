using System;
using Godot;

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

    #endregion Public Methods

    #region Private Methods

    private void ResetPuck()
    {
        // Reset anything from prior use
        LinearVelocity = Vector3.Zero;
        AngularVelocity = Vector3.Zero;
        Rotation = Vector3.Zero;
    }

    #endregion Private Methods
}
