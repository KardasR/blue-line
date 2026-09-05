using System;
using System.Threading.Tasks;

using Godot;

using BlueLine.Goaltender;
using BlueLine.Management;

namespace BlueLine.FrozenRubber;

public partial class Puck : RigidBody3D
{
    #region Properties

    /// <summary>
    /// A node that contains a collection of node3d's that represent different faceoff dots.
    /// </summary>
    public Node FaceoffLocations { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public PuckStates State { get; set; }

    #endregion Properties

    #region Overrides

    public override void _Ready()
    {
        ContactMonitor = true;
        MaxContactsReported = 4;
        BodyEntered += On_BodyEntered;

        GameEvents.Instance.PrepareFaceoff += DropThePuck;

        State = PuckStates.Loose;
    }

    #endregion Overrides

    #region Events

    private void On_BodyEntered(Node body)
    {
        if (State == PuckStates.Shot &&
            body is Goalie)
        {
            PuckSaved();
        }
    }

    #endregion Events

    #region Public Methods

    public void Drop(Vector3 direction, float force)
    {
        if (State != PuckStates.Held)
            return;

        Reparent(GetTree().CurrentScene);
        Freeze = false;
        State = PuckStates.Loose;

        direction.Y = 0.0f;
        direction = direction.Normalized();

        ApplyCentralImpulse(direction * force);
    }

    public void Poke(Vector3 direction, float force)
    {
        direction.Y = 0.0f;

        if (direction.LengthSquared() < 0.001f)
            return;

        direction = direction.Normalized();

        State = PuckStates.Loose;
        //LinearVelocity = direction * force;
        ApplyCentralImpulse(direction * force);
    }

    /// <summary>
    /// Mark when a shot was saved.
    /// </summary>
    public void PuckSaved()
    {
        if (State != PuckStates.Shot)
            return;

        State = PuckStates.Loose;
        GameEvents.Instance.RaisePuckSaved();
    }

    /// <summary>
    /// Shoot the puck in a specific direction.
    /// </summary>
    /// <param name="direction">Where to shoot the puck</param>
    /// <param name="force">How hard to shoot the puck</param>
    public void Shoot(Vector3 direction, float force)
    {
        if (State != PuckStates.Held)
            return;
        
        Reparent(GetTree().CurrentScene);

        Vector3 target = (
            direction - GlobalPosition
        ).Normalized();

        Freeze = false;
        State = PuckStates.Shot;

        GameEvents.Instance.RaiseShotFired(target, force);
        LinearVelocity = target * force;
    }

    public void PassInDirection(Vector3 direction, float force)
    {
        PassInternal(direction.Normalized(), force);
    }

    public void PassToTarget(Vector3 targetPosition, float force)
    {
        Vector3 direction = (targetPosition - GlobalPosition).Normalized();
        PassInternal(direction, force);
    }

    /// <summary>
    /// Freeze the puck and mark it as held so it doesn't fly off the players stick.
    /// </summary>
    /// <param name="grabPoint"></param>
    public void Grab(Node3D grabPoint)
    {
        if (State == PuckStates.Held)
            return;

        ResetPuck();
        
        State = PuckStates.Held;

        Freeze = true;
        GlobalPosition = grabPoint.GlobalPosition;
        
        Reparent(grabPoint);
    }

    /// <summary>
    /// Halts the puck and makes sure that it's standing still.
    /// </summary>
    public void ResetPuck()
    {
        // Reset anything from prior use
        Freeze = false;
        LinearVelocity = Vector3.Zero;
        AngularVelocity = Vector3.Zero;
        Rotation = Vector3.Zero;
        State = PuckStates.Loose;
    }

    /// <summary>
    /// Unfreeze the puck in preparation for a poke check.
    /// </summary>
    public void PrepareForPokeCollision()
    {
        if (State == PuckStates.Loose) 
            return;

        Reparent(GetTree().CurrentScene);

        ResetPuck();
    }

    #endregion Public Methods

    #region Private Methods

    /// <summary>
    /// 
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="force"></param>
    private void PassInternal(Vector3 direction, float force)
    {
        if (State != PuckStates.Held)
            return;

        Reparent(GetTree().CurrentScene);

        Freeze = false;
        State = PuckStates.Pass;

        LinearVelocity = direction * force;

        //GameEvents.Instance.RaisePassMade(direction, force);
    }

    /// <summary>
    /// Drops the puck at a given faceoff dot.
    /// </summary>
    /// <param name="faceoffDot">Where to drop the puck</param>
    /// <exception cref="InvalidOperationException">You must give a node object that is a collection of faceoff dots (area3d's)</exception>
    private void DropThePuck(FaceoffDot faceoffDot)
    {
        if (FaceoffLocations == null)
        {
            throw new InvalidOperationException("No faceoff location node was given. Cannot spawn puck.");
        }
        async Task MakeThemWait(FaceoffDot faceoffDot)
        {
            await ToSignal(GetTree().CreateTimer(2), SceneTreeTimer.SignalName.Timeout);

            Freeze = false;

            GameEvents.Instance.RaisePuckDropped(faceoffDot);
        }

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
            default:
                throw new NotSupportedException($"Faceoff Location: {faceoffDot} is not setup properly. Cannot drop puck");
        }

        // Reset anything from prior use
        ResetPuck();
        Freeze = true;

        GlobalPosition = faceoffLocation;
        
        Task.Run(() => MakeThemWait(faceoffDot));
    }

    #endregion Private Methods
}
