using Godot;

using BlueLine.Management;

namespace BlueLine;

public partial class ShotVisualizer : RigidBody3D
{
    private bool _OkayToMove;

    /// <summary>
    /// What goal the target hovers over.
    /// </summary>
    [Export]
    public Net Net { get; set; }

    /// <summary>
    /// The shot trainer should not move after a goal is scored. This way the player can see where they shot it when they scored.
    /// </summary>
    public bool GoalScored { private get; set; }

    public override void _Ready()
    {
        GameEvents.Instance.ShotFired += ReactToShot;
        GameEvents.Instance.PuckSaved += ReactToSave;
        GameEvents.Instance.GoalScored += ReactToGoal;

        _OkayToMove = true;
    }

    public override void _ExitTree()
    {
        GameEvents.Instance.ShotFired -= ReactToShot;
        GameEvents.Instance.PuckSaved -= ReactToSave;
        GameEvents.Instance.GoalScored -= ReactToGoal;
    }


    private void ReactToShot(Vector3 direction, float force)
    {
        _OkayToMove = false;
    }

    private void ReactToGoal(bool isHomeGoal)
    {
        ResetVisualizer();
    }

    private void ReactToSave()
    {
        ResetVisualizer();
    }

    private void ResetVisualizer()
    {
        _OkayToMove = true;
    }

    public override void _Process(double delta)
    {
        // create a vector2 out of the inputs 
        Vector2 input = Input.GetVector( 
            "move_left", 
            "move_right", 
            "move_forward", 
            "move_backward" 
        );

        if (_OkayToMove)
            GlobalPosition = Net.GetTargetPoint(input);
        else
            GlobalPosition = GlobalPosition;
    }

}