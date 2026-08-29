using Godot;

public partial class PlayerInput : Node
{
    private bool _chargingShot;
    private Vector2 _movement;
    private Vector2 _stickHandle;

    public Vector2 Movement => ApplyDeadzone(_movement);
    public Vector2 StickHandle => ApplyDeadzone(_stickHandle);
    public bool IsShooting { get; private set; }
    public bool IsPassing { get; private set; }
    public bool IsSprinting { get; private set; }
    public bool IsSkatingBackwards { get; private set; }

    public int DeviceId { get; set; } = 0;

    [Export]
    public float Deadzone { private get; set; } = 0.2f;

    public override void _Input(InputEvent @event)
    {
        if (@event.Device != DeviceId)
            return;

        if (@event is InputEventJoypadMotion motion)
        {
            if (motion.Axis == JoyAxis.LeftX)
                _movement.X = motion.AxisValue;

            else if (motion.Axis == JoyAxis.LeftY)
                _movement.Y = motion.AxisValue;

            else if (motion.Axis == JoyAxis.RightX)
                _stickHandle.X = -motion.AxisValue;

            else if (motion.Axis == JoyAxis.RightY)
                _stickHandle.Y = -motion.AxisValue;
        }
        if (@event is InputEventJoypadButton button)
        {
            // if (button.IsActionPressed("shoot")) IsShooting = true;
            // else if (button.IsActionReleased("shoot")) IsShooting = false;

            // if (button.IsActionPressed("pass")) IsPassing = true;
            // else if (button.IsActionReleased("pass")) IsPassing = false;

            // if (button.IsActionPressed("sprint")) IsSprinting = true;
            // else if (button.IsActionReleased("sprint")) IsSprinting = false;

            // if (button.IsActionPressed("skate_backwards")) IsSkatingBackwards = true;
            // else if (button.IsActionReleased("skate_backwards")) IsSkatingBackwards = false;

            IsShooting = button.IsActionPressed("shoot");
            IsPassing = button.IsActionPressed("pass");
            IsSprinting = button.IsActionPressed("sprint");
            IsSkatingBackwards = button.IsActionPressed("skate_backwards");
        }
    }

    /// <summary>
    /// Rescale so output ramps smoothly from 0 right past the deadzone,
    /// instead of jumping straight from 0 to 'deadzone' magnitude.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    private Vector2 ApplyDeadzone(Vector2 input)
    {
        float length = input.Length();
        if (length < Deadzone)
            return Vector2.Zero;

        float rescaled = (length - Deadzone) / (1f - Deadzone);
        return input.Normalized() * rescaled;
    }
}