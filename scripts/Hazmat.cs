using Godot;

public partial class Hazmat : CharacterBody3D
{
    #region Members

    private Vector3 _targetVelocity = Vector3.Zero;

    #endregion Members

    #region Properties

    /// <summary>
    /// Position of where the camera is in the game
    /// </summary>
    [Export]
    Marker3D CameraPosition { get; set; }

    /// <summary>
    /// How fast the player moves in meters per second
    /// </summary>
    [Export]
    public int Speed { get; set; } = 14;

    /// <summary>
    /// The downward acceleration when in the air, in meters per second squared
    /// </summary>
    [Export]
    public uint FallAcceleration { get; set; } = 75;

    #endregion Properties

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

        // future direction of player
        Vector3 direction = Vector3.Zero;

        if (input != Vector2.Zero)
        {
            Vector3 forward = -CameraPosition.GlobalTransform.Basis.Z;
            Vector3 right = CameraPosition.GlobalTransform.Basis.X;

            // Don't allow looking up/down
            forward.Y = 0;
            right.Y = 0;

            forward.Normalized();
            right.Normalized();

            direction = 
                right * input.X +
                forward * input.Y;

            direction.Normalized();
        }

        if (direction != Vector3.Zero)
        {
            // setting the basis property will affect the rotation of the node
            this.Basis = Basis.LookingAt(-direction);
        }

        // ground velocity
        _targetVelocity.Z = direction.Z * Speed;
        _targetVelocity.X = direction.X * Speed;

        // vertical velocity
        if (!IsOnFloor())
        {
            // If in the air, implement gravity to fall towards the floor
            _targetVelocity.Y -= FallAcceleration * (float)delta;
        }

        // move the character
        Velocity = _targetVelocity;
        MoveAndSlide();
    }

    #endregion Overrides
}
