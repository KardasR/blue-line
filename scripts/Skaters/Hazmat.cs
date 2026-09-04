using System;
using Godot;

using BlueLine.FrozenRubber;
using BlueLine.VideoFeed;
using System.Collections.Generic;

namespace BlueLine.Skater;

public partial class Hazmat : CharacterBody3D
{
    #region Members

    private Puck _heldPuck;

    private Node3D _puckHoldPoint;

    private bool _takingShot;

    private uint _shotTimer;

    private Camera3D _cameraPosition => CameraManager.Instance.GetCameraForPlayer(PlayerId);

    private float _passTargetMinDot => Mathf.Cos(Mathf.DegToRad(PassTargetMaxAngle));

    private PokeCheck _pokeChecker;

    #endregion Members

    #region Properties

    /// <summary>
    /// The various attributes of the player.
    /// </summary>
    [Export]
    public PlayerAttributes Attributes { get; set; }

    /// <summary>
    /// The attributes of the world.
    /// </summary>
    [Export]
    public WorldAttributes WorldAttributes { get; set; }

    /// <summary>
    /// Is this player on the home or away team?
    /// </summary>
    public bool HomeTeam { get; set; }

    /// <summary>
    /// ID of the player.
    /// </summary>
    public int PlayerId { get; set; }

    /// <summary>
    /// Which controller the player responds to.
    /// </summary>
    public ControllerInput InputDevice { private get; set; }

    /// <summary>
    /// The different teammates of the player.
    /// </summary>
    public List<Hazmat> Teammates { get; set; }

    #region Puck Settings

    /// <summary>
    /// Which goal the player is shooting at.
    /// </summary>
    [Export]
    public Net AttackingGoal { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Export]
    public float PassTargetMaxAngle { get; set; } = 60.0f;

    #endregion Puck Settings

    #endregion Properties

    #region Events
    
    public void On_Blade_BodyEntered(Puck puck)
    {
        if (puck == _heldPuck) 
            return;

        if (_pokeChecker.IsActivelyPoking)
        {
            puck.PrepareForPokeCollision();
            return;
        }

        if (_heldPuck == null && !_pokeChecker.IsActivelyPoking)
        {
            _heldPuck = puck;
            _heldPuck.Grab(_puckHoldPoint);
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
        if (AttackingGoal == null)
        {
            throw new InvalidOperationException("No attacking goal was given. Cannot shoot on goal.");
        }

        _puckHoldPoint = GetNode<Node3D>("Stick/Pivot Point/Puck Hold Point");
        _pokeChecker = GetNode<PokeCheck>("Stick/Pivot Point");
    }
    /// <summary>
    /// Checks if an input action has been pressed and responds accordingly
    /// </summary>
    /// <param name="delta"></param>
    public override void _PhysicsProcess(double delta) 
    { 
        if (InputDevice != null)
        {
            MovePlayer(delta, InputDevice.Movement);
            StickHandle(delta, InputDevice.StickHandle);
            CheckForPuckAction(InputDevice.Movement, InputDevice.StickHandle);
            CheckForCheckingAction(InputDevice.StickHandle);
        }

        // make sure we catch when a puck has been knocked away
        if (_heldPuck != null && _heldPuck.State != PuckStates.Held)
        {
            _heldPuck = null;
        }
    } 

    #endregion Overrides

    #region Private Methods

    private void CheckForCheckingAction(Vector2 stickAim)
    {
        _pokeChecker.UpdateAim(stickAim);

        if (InputDevice.PokeJustPressed()) _pokeChecker.BeginPoke();
        if (InputDevice.PokeJustReleased()) _pokeChecker.EndPoke();
    }

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
            (right * InputDevice.Movement.X) +
            (forward * -InputDevice.Movement.Y)
        ).Normalized();
    }

    private void CheckForPuckAction(Vector2 aim, Vector2 dangle)
    {
        if (_heldPuck == null)
            return;

        if (InputDevice.IsShooting && !_takingShot)
        {
            _takingShot = true;
            _shotTimer = 0;
        }
        if (InputDevice.IsShooting ||
            dangle.Y < -WorldAttributes.ShotDeadzone)
        {
            _shotTimer += 1;
        }
        if ((!InputDevice.IsShooting && _takingShot) ||
            dangle.Y > WorldAttributes.ShotDeadzone)
        {
            float speed = _shotTimer >= WorldAttributes.SlapshotThreshold ? Attributes.ShotSpeed * Attributes.SlapshotMultiplier : Attributes.ShotSpeed;

            _heldPuck.Shoot(AttackingGoal.GetTargetPoint(aim), speed);

            _heldPuck = null;
            _takingShot = false;
            _shotTimer = 0;
        }

        // TODO: support saucer passes.
        if (InputDevice.IsPassing)
        {
            Vector3 aimDirection = GetCameraRelativeDirection(aim);

            if (aimDirection == Vector3.Zero)
            {
                // If the stick isn't moved then just pass forward.
                _heldPuck.PassInDirection(GlobalTransform.Basis.Z, Attributes.PassSpeed);
            }
            else
            {
                Hazmat target = FindBestPassTarget(aimDirection);
                if (target != null)
                {
                    Vector3 targetVelocity = new Vector3(target.Velocity.X, 0, target.Velocity.Z);
                    Vector3 leadPosition = target._puckHoldPoint.GlobalPosition;

                    if (TryGetPassingTime(target._puckHoldPoint.GlobalPosition, targetVelocity, Attributes.PassSpeed, out float t))
                    {
                        leadPosition = target._puckHoldPoint.GlobalPosition + targetVelocity * t;
                    }

                    _heldPuck.PassToTarget(leadPosition, Attributes.PassSpeed);
                }
                else
                    _heldPuck.PassInDirection(aimDirection, Attributes.PassSpeed);
            }

            _heldPuck = null;
        }
    }

    private Hazmat FindBestPassTarget(Vector3 aimDirection)
    {
        Hazmat best = null;
        float bestScore = _passTargetMinDot;

        foreach (var teammate in Teammates)
        {
            Vector3 toTeammate = teammate._puckHoldPoint.GlobalPosition - GlobalPosition;
            toTeammate.Y = 0;

            if (toTeammate.LengthSquared() < 0.01f) continue;

            float score = aimDirection.Dot(toTeammate.Normalized());
            if (score > bestScore)
            {
                bestScore = score;
                best = teammate;
            }
        }

        return best;
    }

    private void StickHandle(double delta, Vector2 input)
    {
        if (_heldPuck == null)
            return;

        Vector3 offset = _puckHoldPoint.GlobalTransform.Basis.X * input.X;

        offset = offset.LimitLength(Attributes.StickHandleRange);

        Vector3 targetPosition = _puckHoldPoint.GlobalPosition + offset;

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

            Vector3 facingDirection = InputDevice.IsSkatingBackwards ? -direction : direction;

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
            if (InputDevice.IsSprinting) speed = Attributes.SprintSpeed;
            if (InputDevice.IsSkatingBackwards) speed = Attributes.BackwardSpeed;

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

    /// <summary>
    /// Try to predict how far to lead the passing target.
    /// </summary>
    /// <param name="targetPos">Where the target is</param>
    /// <param name="targetVelocity">The velocity of the target</param>
    /// <param name="passSpeed">How fast we are passing the puck</param>
    /// <param name="time"></param>
    /// <returns></returns>
    private bool TryGetPassingTime(
        Vector3 targetPos,
        Vector3 targetVelocity,
        float passSpeed,
        out float time)
    {
        Vector3 toTarget = targetPos - GlobalPosition;

        float targetPassSpeed = targetVelocity.LengthSquared() - passSpeed * passSpeed;
        float angle = 2f * toTarget.Dot(targetVelocity);
        float distance = toTarget.LengthSquared();

        // Target's speed happens to equal the pass speed - linear, not quadratic.
        if (Mathf.Abs(targetPassSpeed) < 0.0001f)
        {
            if (Mathf.Abs(angle) < 0.0001f)
            {
                time = 0f;
                return false;
            }
            time = -distance / angle;
            return time > 0f;
        }

        float discriminant = angle * angle - 4f * targetPassSpeed * distance;
        if (discriminant < 0f)
        {
            // Target is outrunning what the pass can catch up to
            time = 0f;
            return false;
        }

        float sqrtDisc = Mathf.Sqrt(discriminant);
        float t1 = (-angle + sqrtDisc) / (2f * targetPassSpeed);
        float t2 = (-angle - sqrtDisc) / (2f * targetPassSpeed);

        bool found = false;
        float best = float.MaxValue;
        if (t1 > 0f) { best = t1; found = true; }
        if (t2 > 0f && t2 < best) { best = t2; found = true; }

        time = found ? best : 0f;
        return found;
    }
    
    #endregion Private Methods
}