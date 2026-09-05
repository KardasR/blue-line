using Godot;

public partial class ControllerInput : Node
{
    #region Members

    private bool _chargingShot;
    private Vector2 _movement;
    private Vector2 _stickHandle;
    private bool _pokeStarted;
    private bool _pokeEnded;

    #endregion Members

    #region Properties

    public Vector2 Movement => ApplyDeadzone(_movement);
    public Vector2 StickHandle => ApplyDeadzone(_stickHandle);
    public bool IsShooting { get; private set; }
    public bool IsPassing { get; private set; }
    public bool IsSprinting { get; private set; }
    public bool IsSkatingBackwards { get; private set; }
    public bool IsBodyChecking { get; private set; }

    public int DeviceId { get; set; } = 0;

    [Export]
    public float Deadzone { private get; set; } = 0.2f;

    #endregion Properties

    #region Overrides

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
                
            if (motion.IsActionPressed("skate_backwards")) IsSkatingBackwards = true;
            else if (motion.IsActionReleased("skate_backwards")) IsSkatingBackwards = false;
        }
        if (@event is InputEventJoypadButton button)
        {
            if (button.IsActionPressed("shoot")) IsShooting = true;
            else if (button.IsActionReleased("shoot")) IsShooting = false;

            if (button.IsActionPressed("pass")) IsPassing = true;
            else if (button.IsActionReleased("pass")) IsPassing = false;

            if (button.IsActionPressed("sprint")) IsSprinting = true;
            else if (button.IsActionReleased("sprint")) IsSprinting = false;

            if (button.IsActionPressed("poke_check")) _pokeStarted = true;
            else if (button.IsActionReleased("poke_check")) _pokeEnded = true;

            if (button.IsActionPressed("body_check")) IsBodyChecking = true;
            else if (button.IsActionReleased("body_check")) IsBodyChecking = false;
        }
    }

    #endregion Overrides

    #region Is Action Pressed?

    public bool PokeJustPressed()
    {
        if (!_pokeStarted) return false;

        _pokeStarted = false;
        return true;
    }

    public bool PokeJustReleased()
    {
        if (!_pokeEnded) return false;

        _pokeEnded = false;
        return true;
    }

    #endregion Is Action Pressed?

    #region Private Methods

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

    #endregion Private Methods
}