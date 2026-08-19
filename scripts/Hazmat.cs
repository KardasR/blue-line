using System;
using Godot;

public partial class Hazmat : CharacterBody3D
{
    #region Members

    private Puck _heldPuck;

    #endregion Members

    #region Properties

    #region Camera Settings

    /// <summary> 
    /// Position of where the camera is in the game.
    /// </summary> 
    [Export] 
    Camera3D CameraPosition { get; set; }

    #endregion Camera Settings

    #region Movement Settings

    /// <summary> 
    /// How fast the player moves in meters per second.
    /// </summary> 
    [Export] 
    public int Speed { get; set; } = 14;

    /// <summary> 
    /// The downward acceleration when in the air, in meters per second squared.
    /// </summary> 
    [Export] 
    public uint FallAcceleration { get; set; } = 75;

    /// <summary> 
    /// Controls how fast the player turns.
    /// </summary> 
    [Export] 
    public float TurnSpeed { get; set; } = 0.01f;

    /// <summary>
    /// How quick the player comes up to speed;
    /// </summary>
    [Export]
    public float Acceleration { get; set; } = 20.0f;

    /// <summary>
    /// How much the player slows due to ice.
    /// </summary>
    [Export]
    public float IceFriction { get; set; } = 3.0f;

    /// <summary>
    /// How quick the player will come to a stop when going against their movement.
    /// </summary>
    [Export]
    public float StoppingAcceleration { get; set; } = 60.0f;

    #endregion Movement Settings

    #region Puck Settings

    /// <summary>
    /// Position of where to hold the puck.
    /// </summary>
    [Export]
    public Node3D PuckHoldPoint { get; set; }

    /// <summary>
    /// How hard the player shoots.
    /// </summary>
    [Export]
    public float ShotSpeed { get; set; } = 40.0f;

    /// <summary>
    /// How hard the player passes the puck.
    /// </summary>
    [Export]
    public float PassSpeed { get; set; } = 20.0f;

    /// <summary>
    /// Which goal the player is shooting at.
    /// </summary>
    [Export]
    public Goal AttackingGoal { get; set; }

    #endregion Puck Settings

    #endregion Properties

    #region Events
    
    public void On_Blade_BodyEntered(Node3D body)
    {
        if (PuckHoldPoint == null)
        {
            throw new InvalidOperationException("No puck hold point was given. Puck cannot be grabbed.");
        }

        if (body is Puck puck && _heldPuck == null)
        {
            puck.Grab(PuckHoldPoint);
            _heldPuck = puck;
        }
    }

    #endregion Events

    #region Overrides

    /// <summary>
    /// Checks if an input action has been pressed and responds accordingly
    /// </summary>
    /// <param name="delta"></param>
    public override void _PhysicsProcess(double delta) 
    { 
        // create a vector2 out of the inputs 
        Vector2 input = Input.GetVector( 
            "move_left", 
            "move_right", 
            "move_forward", 
            "move_backward" 
        ); 
        
        MovePlayer(delta, input);
        CheckForPuckAction(input);
    } 

    #endregion Overrides

    #region Private Methods

    private void CheckForPuckAction(Vector2 aim)
    {
        if (_heldPuck == null)
            return;

        if (Input.IsActionPressed("shoot"))
        {
            _heldPuck.Shoot(AttackingGoal.GetTargetPoint(aim), ShotSpeed);

            _heldPuck = null;
        }
        if (Input.IsActionPressed("pass"))
        {
            _heldPuck.Shoot(Velocity, PassSpeed);

            _heldPuck = null;
        }
    }

    private void MovePlayer(double delta, Vector2 input)
    {
        // future direction of player
        Vector3 direction = Vector3.Zero; 
        if (input != Vector2.Zero) 
        { 
            Vector3 forward = -CameraPosition.GlobalTransform.Basis.Z; 
            Vector3 right = CameraPosition.GlobalTransform.Basis.X; 
            
            // Don't allow looking up/down forward.
            forward.Y = 0; 
            right.Y = 0; 

            forward = forward.Normalized(); 
            right = right.Normalized(); 

            direction = (
                (right * input.X) + 
                (forward * -input.Y)
            ).Normalized();

            float targetAngle = Mathf.Atan2( 
                direction.X, 
                direction.Z 
            ); 

            Rotation = new Vector3(
                Rotation.X,
                Mathf.LerpAngle(
                    Rotation.Y,
                    targetAngle,
                    TurnSpeed * (float)delta
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
            Vector3 desiredVelocity = direction * Speed;

            float accel = Acceleration;

            // Are we trying to move against our current momentum?
            if (horizontalVelocity.Dot(direction) < 0)
            {
                accel = StoppingAcceleration;
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
                IceFriction * (float)delta
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
                Velocity.Y - FallAcceleration * (float)delta,
                Velocity.Z
            );
        }
        
        MoveAndSlide(); 
    }

    #endregion Private Methods
}
