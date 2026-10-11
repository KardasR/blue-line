using BlueLine.Management;
using Godot;

namespace BlueLine.Skaters;

public partial class AiSkater : Node
{
    private bool _canMove =>_skater?.InputDevice is AiInput &&
                            _skater?.Assignment == Management.Positions.LeftDefense && 
                            !_skater.IsHomeTeam;
    private Skater _skater => GetParent<Skater>();

    private AiInput AiController => _skater.InputDevice as AiInput;

    public override void _PhysicsProcess(double delta)
    {
        if (!_canMove)
        {
            base._PhysicsProcess(delta);
            return;
        }

        SkateToTarget();
    }

    public void SkateToTarget()
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