using Godot;

using BlueLine.Management;

namespace BlueLine;

public partial class ShotVisualizer : RigidBody3D
{
    private bool _okayToMove;

    private ControllerInput _input;

    /// <summary>
    /// What goal the target hovers over.
    /// </summary>
    [Export]
    public Net Net { get; set; }

    /// <summary>
    /// The shot trainer should not move after a goal is scored. This way the player can see where they shot it when they scored.
    /// </summary>
    public bool GoalScored { private get; set; }

    public ControllerInput Controller { 
        private get => _input; 
        set 
        {
            // make sure we only set this once
            if (_input == null)
            {
                _input = value;
                _okayToMove = true;
            }
        } 
    }

    public override void _Ready()
    {
        GameEvents.Instance.ShotFired += ReactToShot;
        GameEvents.Instance.PuckSaved += ReactToSave;
        GameEvents.Instance.GoalScored += ReactToGoal;
        GameEvents.Instance.PrepareFaceoff += ReactToFaceoffPrep;
    }

    private void ReactToFaceoffPrep(FaceoffDot faceoffDot)
    {
        ResetVisualizer();
    }

    private void ReactToShot(Vector3 direction, float force)
    {
        _okayToMove = false;
    }

    private void ReactToGoal(bool isHomeGoal)
    {
        _okayToMove = false;
    }

    private void ReactToSave()
    {
        ResetVisualizer();
    }

    private void ResetVisualizer()
    {
        _okayToMove = true;
    }

    public override void _Process(double delta)
    {
        if (_okayToMove)
            GlobalPosition = Net.GetTargetPoint(Controller.Movement);
        else
            GlobalPosition = GlobalPosition;
    }

}