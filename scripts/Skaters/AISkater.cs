using BlueLine.Management;
using Godot;

namespace BlueLine.Skaters;

public partial class AiSkater : Node
{
    private bool _canControl =>_skater?.InputDevice is AiInput;
    private Skater _skater => GetParent<Skater>();

    private AiInput AiController => _skater.InputDevice as AiInput;

    public override void _PhysicsProcess(double delta)
    {
        if (!_canControl)
        {
            // see if we need to make an aiinput
            if (_skater?.InputDevice == null)
            {
                _skater.InputDevice = new AiInput();
            }
        }

        // move here
    }

    private void SkateToTarget()
    {
        if (AiController == null) 
            return;

        Vector3 direction = _skater.SkatingHoldPoint.GlobalPosition - MatchStatus.Instance.SpawnedPuck.GlobalPosition;
        direction.Y = 0;

        if (direction.LengthSquared() < 0.01f)
            return;

        direction = direction.Normalized();

        AiController.UpdateMovement(new() { X=direction.X, Y=direction.Z });
    }
}