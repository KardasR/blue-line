using System;
using Godot;

public partial class Goal : MeshInstance3D
{
    #region Properties

    /// <summary>
    /// How wide the goal is.
    /// </summary>
    [Export]
    public float Width { get; set; } = 0.2f;

    /// <summary>
    /// How tall the goal is.
    /// </summary>
    [Export]
    public float Height { get; set; } = 5.3f;

    /// <summary>
    /// How much stick movement is ignored in the center of the joystick.
    /// </summary>
    [Export]
    public float AimDeadzone { get; set; } = 0.15f;

    /// <summary>
    /// 
    /// </summary>
    [Export]
    public Node3D AimTarget { get; set; }

    #endregion Properties

    #region Public Methods

    public Vector3 GetTargetPoint(Vector2 aim)
    {
        if (AimTarget == null)
        {
            throw new InvalidOperationException("No aim target was given to orient aiming.");
        }

        float magnitude = aim.Length();

        // Ignore small controller movement.
        if (magnitude < AimDeadzone)
        {
            aim = Vector2.Zero;
        }
        else
        {
            // Remap the range so that the deadzone becomes 0
            // and the remainder of the stick range becomes 0-1.
            float remappedMagnitude =
                (magnitude - AimDeadzone) /
                (1.0f - AimDeadzone);

            remappedMagnitude = Mathf.Clamp(
                remappedMagnitude,
                0.0f,
                1.0f
            );

            aim = aim.Normalized() * remappedMagnitude;

            // Convert the circular stick range into a square range
            // while preserving how far the stick is actually pushed.
            float maxComponent = Mathf.Max(
                Mathf.Abs(aim.X),
                Mathf.Abs(aim.Y)
            );

            if (maxComponent > 0.0f)
            {
                aim *= 1.0f / maxComponent;
                aim *= remappedMagnitude;
            }
        }

        float x = -aim.X * (Width / 2.0f);
        float z = -aim.Y * (Height / 2.0f);

        Vector3 localPoint = new Vector3(
            x,
            0,
            z
        );

        return AimTarget.GlobalTransform * localPoint;
    }

    #endregion Public Methods
}
