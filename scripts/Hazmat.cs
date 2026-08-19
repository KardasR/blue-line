using Godot;

public partial class Hazmat : CharacterBody3D
{
    #region Properties

    /// <summary> 
    /// Position of where the camera is in the game.
    /// </summary> 
    [Export] 
    Camera3D CameraPosition { get; set; }

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
        

        Velocity = new Vector3(
            direction.X * Speed,
            Velocity.Y,
            direction.Z * Speed
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

    #endregion Overrides
}
