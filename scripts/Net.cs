using System;
using Godot;

using BlueLine.Management;

namespace BlueLine;

public partial class Net : MeshInstance3D
{
    #region Members

    private float _puckX;

    #endregion Members

    #region Properties

    [ExportGroup("Dimensions")]
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

    [ExportGroup("Aiming")]
    /// <summary>
    /// How much stick movement is ignored in the center of the joystick.
    /// </summary>
    [Export]
    public float AimDeadzone { get; set; } = 0.15f;

    /// <summary>
    /// Aim target that sits in the net.
    /// </summary>
    [Export]
    public Node3D AimTarget { get; set; }

    /// <summary>
    /// Is this the home goal?
    /// </summary>
    [Export]
    public bool HomeNet { get; set; }

    #endregion Properties

    #region Events

    public void On_Goal_BodyEntered(Node3D puck)
    {
        _puckX = puck.GlobalPosition.X;
    }

    public void On_Goal_BodyExited(Node3D puck)
    {
        if ((HomeNet &&
                puck.GlobalPosition.X > _puckX) ||
            (!HomeNet &&
                puck.GlobalPosition.X < _puckX))
        {
            // a goal has been scored.
            GameEvents.Instance.RaiseGoalScored(!HomeNet);
        }

        _puckX = 0;
    }

    #endregion Events

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

        Vector3 localPoint = new Vector3(
            HomeNet ? aim.X * (Width / 2.0f) : -aim.X * (Width / 2.0f),
            0,
            -aim.Y * (Height / 2.0f)
        );

        return AimTarget.GlobalTransform * localPoint;
    }

    #endregion Public Methods
}