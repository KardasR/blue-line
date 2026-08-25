using Godot;

namespace BlueLine;

public partial class ShotVisualizer : RigidBody3D
{
    /// <summary>
    /// What goal the target hovers over.
    /// </summary>
    [Export]
    public Net Net { get; set; }

    /// <summary>
    /// The shot trainer should not move after a goal is scored. This way the player can see where they shot it when they scored.
    /// </summary>
    public bool GoalScored { private get; set; }

    public override void _Process(double delta)
    {
        // create a vector2 out of the inputs 
        Vector2 input = Input.GetVector( 
            "move_left", 
            "move_right", 
            "move_forward", 
            "move_backward" 
        );

        if (!GoalScored)
            GlobalPosition = Net.GetTargetPoint(input);
        else
            GlobalPosition = GlobalPosition;
    }

}