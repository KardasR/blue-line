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
    }

    /// <summary>
    /// Drops the puck at a given faceoff dot.
    /// </summary>
    /// <param name="faceoffDot">Where to drop the puck</param>
    /// <exception cref="InvalidOperationException">You must give a node object that is a collection of faceoff dots (area3d's)</exception>
    private void DropThePuck(FaceoffDot faceoffDot)
    {
        async Task MakeThemWait(FaceoffDot faceoffDot)
        {
            await ToSignal(GetTree().CreateTimer(2), SceneTreeTimer.SignalName.Timeout);

            Freeze = false;

            GameEvents.Instance.RaisePuckDropped(faceoffDot);
        }

        // Reset anything from prior use
        ResetPuck();
        Freeze = true;

        GlobalPosition = FaceoffLineup.Instance.FaceoffLocations[faceoffDot].GlobalPosition;
        
        Task.Run(() => MakeThemWait(faceoffDot));
    }

    #endregion Private Methods
}
