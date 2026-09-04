using System;
using Godot;

using BlueLine.FrozenRubber;

namespace BlueLine.Skater;

public partial class PokeCheck : Node3D
{
    #region Members
    
    private float _restYaw;
    private float _restPitch;
    private float _pokeYaw;
    private bool _buttonHeld;
    private Vector2 _currentStickInput;
    private Vector3 _previousBladePosition;
    private PokeState _state = PokeState.Resting;

    #endregion Members

    #region Enums

    private enum PokeState { Resting, Extending, Holding, Retracting }

    #endregion Enums

    #region Properties
    
    [Export] 
    public float ExtendSpeed = 12f;

    [Export] 
    public float TrackingSpeed = 8f;
    
    [Export] 
    public float RetractSpeed = 10f;

    [Export] 
    public float ArrivalThreshold = 2f;

    [Export] 
    public float StickDeadzone = 0.2f;

    [Export] 
    public float MaxSweepAngle = 90.0f;

    [Export] 
    public float NeutralSweepAngle = 90.0f;
    
    [Export]
    public Marker3D StickPhysicsTarget { private get; set; }

    [Export]
    public AnimatableBody3D StickPhysics { get; set; }

    [Export]
    public ShapeCast3D PokeShapeCast { private get; set; }

    [Export]
    public Marker3D BladeCenter { private get; set; }

    [Export]
    public float PokeForceMultiplier { get; set; } = 1.0f;

    [Export]
    public float MinimumPokeForce { get; set; } = 5.0f;

    [Export]
    public float MaximumPokeForce { get; set; } = 30.0f;

    [Export]
    public float MinimumPokeSpeed { get; set; } = 1.0f;

    public bool IsActivelyPoking => _state is PokeState.Extending or PokeState.Holding;

    public Vector3 CurrentWorldDirection => GlobalTransform.Basis.Z; // match the stick's local forward axis

    #endregion Properties

    #region Overrides

    public override void _Ready()
    {
        if (StickPhysics == null)
        {
            throw new InvalidOperationException("No Stick Physics was given.");
        }
        if (StickPhysicsTarget == null)
        {
            throw new InvalidOperationException("No Stick Physics Target was given.");
        }
        if (PokeShapeCast == null)
        {
            throw new InvalidOperationException("No Poke Shape Cast was given.");
        }

        _restYaw = Rotation.Y;
        _restPitch = Rotation.X;

        _previousBladePosition = BladeCenter.GlobalPosition;
    }

    public override void _PhysicsProcess(double delta)
    {
        UpdatePokeState(delta);
        UpdatePhysicsStick();
        CheckForPuckSweep(delta);
    }

    #endregion Overrides

    #region Public Methods

    public void UpdateAim(Vector2 stickHandleInput)
    {
        _currentStickInput = stickHandleInput;
    }

    public void BeginPoke()
    {
        if (_state != PokeState.Resting)
            return;

        _pokeYaw = ComputeTargetYaw(_currentStickInput);

        _buttonHeld = true;
        _state = PokeState.Extending;
    }

    public void EndPoke()
    {
        _buttonHeld = false;

        if (_state == PokeState.Holding ||
            _state == PokeState.Extending)
        {
            _state = PokeState.Retracting;
        }
    }

    #endregion Public Methods

    #region Private Methods

    private void CheckForPuckSweep(double delta)
    {
        if (!IsActivelyPoking)
            return;

        Vector3 currentPosition = BladeCenter.GlobalPosition;

        Vector3 movement = currentPosition - _previousBladePosition;

        float movementSpeed = movement.Length() / (float)delta;

        if (movementSpeed < MinimumPokeSpeed)
        {
            _previousBladePosition = currentPosition;
            return;
        }

        if (movement.LengthSquared() < 0.0001f)
        {
            _previousBladePosition = currentPosition;
            return;
        }

        PokeShapeCast.GlobalPosition = _previousBladePosition;
        PokeShapeCast.TargetPosition = movement;

        PokeShapeCast.ForceShapecastUpdate();

        for (int i = 0; i < PokeShapeCast.GetCollisionCount(); i++)
        {
            if (PokeShapeCast.GetCollider(i) is Puck puck)
            {
                Vector3 pokeDirection = movement.Normalized();

                float pokeForce =
                    Mathf.Clamp(
                        movementSpeed * PokeForceMultiplier,
                        MinimumPokeForce,
                        MaximumPokeForce
                    );

                puck.Poke(pokeDirection, pokeForce);

                break;
            }
        }

        _previousBladePosition = currentPosition;
    }
    
    private void UpdatePokeState(double delta)
    {
        if (_buttonHeld &&
            IsActivelyPoking &&
            _currentStickInput.Length() > StickDeadzone)
        {
            _pokeYaw = ComputeTargetYaw(_currentStickInput);
        }

        switch (_state)
        {
            case PokeState.Resting:
                break;

            case PokeState.Extending:
                RotateTowards(_pokeYaw, ExtendSpeed, delta);

                if (AngleDiffDegrees(Rotation.Y, _pokeYaw) < ArrivalThreshold)
                {
                    SetRotation(_pokeYaw);

                    _state = _buttonHeld ? PokeState.Holding : PokeState.Retracting;
                }

                break;

            case PokeState.Holding:
                RotateTowards( _pokeYaw, TrackingSpeed, delta);

                if (!_buttonHeld)
                    _state = PokeState.Retracting;

                break;

            case PokeState.Retracting:
                RotateTowards(_restYaw, RetractSpeed, delta);

                if (AngleDiffDegrees(Rotation.Y, _restYaw) < ArrivalThreshold)
                {
                    SetRotation(_restYaw, _restPitch);
                    _state = PokeState.Resting;
                }

                break;
        }
    }

    private void UpdatePhysicsStick()
    {
        StickPhysics.GlobalTransform = StickPhysicsTarget.GlobalTransform;
    }

    private float ComputeTargetYaw(Vector2 stickInput)
    {
        float pokeYaw = _restYaw - Mathf.DegToRad(NeutralSweepAngle);

        if (stickInput.Length() <= StickDeadzone)
            return -pokeYaw;

        return -(pokeYaw - stickInput.X * Mathf.DegToRad(MaxSweepAngle));
    }

    private void RotateTowards(float targetYaw, float speed, double delta)
    {
        float newYaw = Mathf.LerpAngle(Rotation.Y, targetYaw, speed * (float)delta);

        SetRotation(newYaw);
    }

    private void SetRotation(float yaw, float pitch = 0.0f)
    {
        Rotation = new Vector3(pitch, yaw, 0);
    }

    private static float AngleDiffDegrees(float a, float b)
    {
        return Mathf.RadToDeg(
            Mathf.Abs(
                Mathf.AngleDifference(a, b)
            )
        );
    }

    #endregion Private Methods

}