using Godot;

using BlueLine.Management;
using System.Threading.Tasks;
using BlueLine.Skater;

namespace BlueLine;

public partial class ShotVisualizer : RigidBody3D
{
    #region Members

    private bool _okayToMove;

    private ControllerInput _input;

    #endregion Members

    #region Properties

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
        get => _input; 
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

    #endregion Properties

    #region Overrides

    public override void _Ready()
    {
        GameEvents.Instance.ShotFired += ReactToShot;
        GameEvents.Instance.PuckSaved += ReactToSave;
        GameEvents.Instance.GoalScored += ReactToGoal;
        GameEvents.Instance.PrepareFaceoff += ReactToFaceoffPrep;
        GameEvents.Instance.NewPuckCarrier += ReactToNewPuckCarrier;
    }

    public override void _Process(double delta)
    {
        if (_okayToMove)
            GlobalPosition = Net.GetTargetPoint(Controller.Movement);
        else
            GlobalPosition = GlobalPosition;
    }

    #endregion Overrides

    #region Private Methods

    private void ReactToNewPuckCarrier(Hazmat skater)
    {
        // TODO: I need to make sure the skater has a controller attached

        if (Net.HomeNet != skater.HomeTeam)
        {
            _okayToMove = false;
            //_input = skater.InputDevice;
            _okayToMove = true;
        }
    }

    private void ReactToFaceoffPrep(FaceoffDot faceoffDot)
    {
        if (Controller != null)
            ResetVisualizer();
        else
            HideVisualizer();
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
        // lets wait a second to reset the visualizer
        _ = WaitToReset(1);
    }

    private async Task WaitToReset(float seconds)
    {
        await ToSignal(GetTree().CreateTimer(seconds), SceneTreeTimer.SignalName.Timeout);

        ResetVisualizer();
    }

    private void ResetVisualizer()
    {
        if (Controller != null)
            _okayToMove = true;
    }

    private void HideVisualizer()
    {
        if (Visible) Visible = false;
    }

    #endregion Private Methods
    
}