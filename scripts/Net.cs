using System;
using Godot;

using BlueLine.Management;
using BlueLine.FrozenRubber;

namespace BlueLine;

public partial class Net : MeshInstance3D
{
    #region Members

    private Vector3 _prevPuckPosition;
    private bool _hasPrevPosition;
    private bool _goalScored;

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

    /// <summary>
    /// The puck that the net will try to detect goals with.
    /// </summary>
    [Export]
    public Puck PuckToTrack { get; set; }

    #endregion Properties

    #region Events

    public void On_Faceoff(FaceoffDot _)
    {
        _goalScored = false;
    }

    #endregion Events

    #region Overrides

    public override void _Ready()
    {
        GameEvents.Instance.PrepareFaceoff += On_Faceoff;
    }

    public override void _ExitTree()
    {
        GameEvents.Instance.PrepareFaceoff -= On_Faceoff;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (PuckToTrack == null || _goalScored)
        {
            if (PuckToTrack != null) 
                _prevPuckPosition = PuckToTrack.GlobalPosition;
                
            return;
        }

        Vector3 current = PuckToTrack.GlobalPosition;

        if (_hasPrevPosition)
            CheckCrossedLine(_prevPuckPosition, current);

        _prevPuckPosition = current;
        _hasPrevPosition = true;
    }


    #endregion Overrrides

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

    #region Private Methods

    private void CheckCrossedLine(Vector3 previous, Vector3 current)
    {
        float lineX = GlobalPosition.X;

        bool wasNotIn = HomeNet ? previous.X <= lineX : previous.X >= lineX;
        bool isNowIn  = HomeNet ? current.X  >  lineX : current.X  <  lineX;

        if (!(wasNotIn && isNowIn)) 
            return;

        float denom = current.X - previous.X;
        float t = Mathf.IsZeroApprox(denom) ? 0f : Mathf.Clamp((lineX - previous.X) / denom, 0f, 1f);
        Vector3 crossingPoint = previous.Lerp(current, t);

        if (IsWithinGoalBounds(crossingPoint))
        {
            _goalScored = true;
            GameEvents.Instance.RaiseGoalScored(!HomeNet);
        }
    }

    private bool IsWithinGoalBounds(Vector3 worldPoint)
    {
        Vector3 local = AimTarget.ToLocal(worldPoint);
        return Mathf.Abs(local.X) <= Width / 2f && Mathf.Abs(local.Z) <= Height / 2f;
    }

    #endregion Private Methods
}