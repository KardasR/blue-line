using BlueLine.FrozenRubber;
using Godot;

namespace BlueLine.Skaters;

public partial class AiSkater : Node
{
    [Export]
    public Puck PuckToTrack { get; set; }

    public override void _PhysicsProcess(double delta)
    {
        if (PuckToTrack != null)
        {
            
        }
    }
}