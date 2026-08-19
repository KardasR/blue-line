using Godot;

public partial class ShotVisualizer : RigidBody3D
{
    /// <summary>
    /// What goal the target hovers over.
    /// </summary>
    [Export]
    public Goal Net { get; set; }

    public override void _Process(double delta)
    {
        // create a vector2 out of the inputs 
        Vector2 input = Input.GetVector( 
            "move_left", 
            "move_right", 
            "move_forward", 
            "move_backward" 
        ); 

        GlobalPosition = Net.GetTargetPoint(input);
    }

}
