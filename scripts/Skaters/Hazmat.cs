using System;
using Godot;

using BlueLine.FrozenRubber;
using BlueLine.VideoFeed;

namespace BlueLine.Skater;

public partial class Hazmat : CharacterBody3D
{
    #region Members

    private Puck _heldPuck;

    private bool _takingShot;

    private uint _shotTimer;

    private Vector2 _moveInput = Vector2.Zero;

    private bool _isSprinting;

    private bool _isSkatingBackwards;

    private bool _passJustPressed;

    private Camera3D _cameraPosition => CameraManager.Instance.GetCameraForPlayer(PlayerId);

    private PlayerInput _input;

    #endregion Members

    #region Properties

    [Export]
    public PlayerAttributes Attributes { get; set; }

    [Export]
    public WorldAttributes WorldAttributes { get; set; }

    public bool HomeTeam { get; set; }

    public int DeviceId { get; set; }

    public int PlayerId { get; set; }

    #region Puck Settings

    /// <summary>
    /// Position of where to hold the puck.
    /// </summary>
    [Export]
    public Node3D PuckHoldPoint { get; set; }

    /// <summary>
    /// Which goal the player is shooting at.
    /// </summary>
    [Export]
    public Net AttackingGoal { get; set; }

    #endregion Puck Settings

    #endregion Properties

    #region Events
    
    public void On_Blade_BodyEntered(Node3D puck)
    {
        if (_heldPuck == null)
        {
            _heldPuck = (Puck)puck;
            _heldPuck.Grab(PuckHoldPoint);
        }
    }

    #endregion Events

    #region Overrides

    public override void _Ready()
    {
        if (Attributes == null)
        {
            throw new InvalidOperationException("Player attributes were not given. Cannot do anything.");
        }
        if (WorldAttributes == null)
        {
            throw new InvalidOperationException("World Attributes was not given. Cannot skate");
        }
        if (PuckHoldPoint == null)
        {
            throw new InvalidOperationException("No puck hold point was given. Puck cannot be grabbed.");
        }
        if (AttackingGoal == null)
        {
            throw new InvalidOperationException("No attacking goal was given. Cannot shoot on goal.");
        }

        _input = GetNode<PlayerInput>("PlayerInput");
        _input.DeviceId = DeviceId;
    }
    /// <summary>
    /// Checks if an input action has been pressed and responds accordingly
    /// </summary>
    /// <param name="delta"></param>
    public override void _PhysicsProcess(double delta) 
    { 
        MovePlayer(delta, _input.Movement);
        StickHandle(delta, _input.StickHandle);
        CheckForPuckAction(_input.Movement, _input.StickHandle);
    } 

    #endregion Overrides

    #region Private Methods

    private Vector3 GetCameraRelativeDirection(Vector2 input)
    {
        if (input == Vector2.Zero)
            return Vector3.Zero;

        Vector3 forward = -_cameraPosition.GlobalTransform.Basis.Z;
        Vector3 right = _cameraPosition.GlobalTransform.Basis.X;

        forward.Y = 0;
        right.Y = 0;

        forward = forward.Normalized();
        right = right.Normalized();

        return (
            (right * _input.Movement.X) +
            (forward * -_input.Movement.Y)
        ).Normalized();
    }

    private void CheckForPuckAction(Vector2 aim, Vector2 dangle)
    {
        if (_heldPuck == null)
            return;

        if (_input.IsShooting && !_takingShot)
        {
            _takingShot = true;
            _shotTimer = 0;
        }
        if (_input.IsShooting ||
            dangle.Y < -WorldAttributes.ShotDeadzone)
        {
            _shotTimer += 1;
        }
        if ((!_input.IsShooting && _takingShot) ||
            dangle.Y > WorldAttributes.ShotDeadzone)
        {
            float speed = _shotTimer >= WorldAttributes.SlapshotThreshold ? Attributes.ShotSpeed * Attributes.SlapshotMultiplier : Attributes.ShotSpeed;

            _heldPuck.Shoot(AttackingGoal.GetTargetPoint(aim), speed);

            _heldPuck = null;
            _takingShot = false;
            _shotTimer = 0;
        }

        if (_input.IsPassing)
        {
            _heldPuck.PassInDirection(GetCameraRelativeDirection(aim), Attributes.PassSpeed);

            _heldPuck = null;
        }
    }

    private void StickHandle(double delta, Vector2 input)
    {
        if (_heldPuck == null)
            return;

        Vector3 offset = PuckHoldPoint.GlobalTransform.Basis.X * input.X;

        offset = offset.LimitLength(Attributes.StickHandleRange);

        Vector3 targetPosition = PuckHoldPoint.GlobalPosition + offset;

        _heldPuck.GlobalPosition = _heldPuck.GlobalPosition.Lerp(
            targetPosition,
            Attributes.StickHandleSpeed * (float)delta
        );
    }

    private void MovePlayer(double delta, Vector2 input)
    {
        // future direction of player
        Vector3 direction = Vector3.Zero; 
        if (input != Vector2.Zero) 
        {
            direction = GetCameraRelativeDirection(input);

            Vector3 facingDirection = _input.IsSkatingBackwards ? -direction : direction;

            float targetAngle = Mathf.Atan2( 
                facingDirection.X, 
                facingDirection.Z 
            );

            Rotation = new Vector3(
                Rotation.X,
                Mathf.LerpAngle(
                    Rotation.Y,
                    targetAngle,
                    Attributes.TurnSpeed * (float)delta
                ),
                Rotation.Z
            );
        }        

        Vector3 horizontalVelocity = new Vector3(
            Velocity.X,
            0,
            Velocity.Z
        );

        if (direction != Vector3.Zero)
        {
            float speed = Attributes.SkatingSpeed;
            if (_input.IsSprinting) speed = Attributes.SprintSpeed;
            if (_input.IsSkatingBackwards) speed = Attributes.BackwardSpeed;

            Vector3 desiredVelocity = direction * speed;

            float accel = Attributes.Acceleration;

            // Are we trying to move against our current momentum?
            if (horizontalVelocity.Dot(direction) < 0)
            {
                accel = Attributes.Deceleration;
            }

            horizontalVelocity = horizontalVelocity.MoveToward(
                desiredVelocity,
                accel * (float)delta
            );
        }
        else
        {
            horizontalVelocity = horizontalVelocity.MoveToward(
                Vector3.Zero,
                WorldAttributes.IceFriction * (float)delta
            );
        }

        Velocity = new Vector3(
            horizontalVelocity.X,
            Velocity.Y,
            horizontalVelocity.Z
        );

        if (!IsOnFloor())
        {
            Velocity = new Vector3(
                Velocity.X,
                Velocity.Y - WorldAttributes.FallAcceleration * (float)delta,
                Velocity.Z
            );
        }
        
        MoveAndSlide(); 
    }
    
    #endregion Private Methods
}