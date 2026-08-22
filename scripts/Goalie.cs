using Godot;

public partial class Goalie : CharacterBody3D
{
    #region Properties

    /// <summary>
    /// The puck that the goalie tries to save.
    /// </summary>
    public Puck PuckToTrack { get; set; }

    /// <summary>
    /// The goal that the goalie stands in front of.
    /// </summary>
    [Export]
    public Node3D GoalToDefend { get; set; }

    #endregion Properties

    #region Overrides

    public override void _Ready()
    {
        // TODO: track where the puck is and maintain a decent angle. Also challenge the shooter.
        
        
    }


    #endregion Overrides
}
