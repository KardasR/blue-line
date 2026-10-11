using Godot;

namespace BlueLine.Skaters;

public partial class BaseInput : Node
{
    #region Properties

    public Vector2 Movement;
    public Vector2 StickHandle;
    public bool IsShooting { get; protected set; }
    public bool IsPassing { get; protected set; }
    public bool IsSprinting { get; protected set; }
    public bool IsSkatingBackwards { get; protected set; }
    public bool IsBodyChecking { get; protected set; }

    public int DeviceId { get; set; } = 0;

    [Export]
    public float Deadzone { protected get; set; } = 0.2f;

    #endregion Properties
}