using Godot;

namespace BlueLine.Skaters;

public partial class ControllerInput : BaseInput
{
    #region Members

    private Vector2 _movement;
    private Vector2 _stickHandle;

    #endregion Members

    #region Overrides

    public override void _Input(InputEvent @event)
    {
        if (@event.Device != DeviceId)
            return;

        if (@event is InputEventJoypadMotion motion)
        {
            if (motion.Axis == JoyAxis.LeftX)
            {
                _movement.X = motion.AxisValue;
                Movement = ApplyDeadzone(_movement);
            }

            else if (motion.Axis == JoyAxis.LeftY)
            {
                _movement.Y = motion.AxisValue;
                Movement = ApplyDeadzone(_movement);
            }

            else if (motion.Axis == JoyAxis.RightX)
            {
                _stickHandle.X = -motion.AxisValue;
                StickHandle = ApplyDeadzone(_stickHandle);
            }

            else if (motion.Axis == JoyAxis.RightY)
            {
                _stickHandle.Y = -motion.AxisValue;
                StickHandle = ApplyDeadzone(_stickHandle);
            }
                
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

            if (button.IsActionPressed("body_check")) IsBodyChecking = true;
            else if (button.IsActionReleased("body_check")) IsBodyChecking = false;
        }
    }

    #endregion Overrides

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