using System;
using System.Threading.Tasks;

using Godot;

using BlueLine.Goaltender;
using BlueLine.FrozenRubber;

namespace BlueLine.Management;

public partial class MainNode : Node
{
    #region Members

    private ushort _homeScore = 0;

    private ushort _awayScore = 0;

    private Puck _spawnedPuck;

    private Label _homeScoreLbl;

    private Label _awayScoreLbl;

    private ShotVisualizer _shotVisualizer;

    #endregion Members

    #region Properties

    /// <summary>
    /// Scene that contains the puck we're going to use.
    /// </summary>
    [Export]
    public PackedScene PuckScene { get; set; }

    /// <summary>
    /// Home Goal.
    /// </summary>
    [Export]
    public Net HomeNet { get; set; }

    /// <summary>
    /// Away Goal.
    /// </summary>
    [Export]
    public Net AwayNet { get; set; }

    /// <summary>
    /// How long until the game respawns the puck after a goal.
    /// </summary>
    [Export]
    public float ResetTimer { get; set; } = 3.0f;

    #endregion Properties

    #region Events

    /// <summary>
    /// React to a goal being scored.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void On_GoalScored(bool homeGoal)
    {
        GoalScored(homeGoal);
    }

    #endregion Events

    #region Overrides

    public override void _Ready()
    {
        if (PuckScene == null)
        {
            throw new InvalidOperationException("Puck Scene was not given. Cannot spawn puck");
        }
        if (HomeNet == null)
        {
            throw new InvalidOperationException("Home Goal was not setup. Cannot react to a goal.");
        }
        if (AwayNet == null)
        {
            throw new InvalidOperationException("Away Goal was not setup. Cannot react to a goal.");
        }

        // setup refs
        _homeScoreLbl = GetNode<Label>("UI/Score Board/Home Score");
        _awayScoreLbl = GetNode<Label>("UI/Score Board/Away Score");
        _shotVisualizer = GetNode<ShotVisualizer>("Shot Visualizer");

        // subscribe to events
        GameEvents.Instance.GoalScored += On_GoalScored;

        // create and add the puck to the scene
        // TODO: do this for the player too?
        Puck puck = PuckScene.Instantiate<Puck>();
        _spawnedPuck = puck;

        AddChild(puck);
        GetNode<Goalie>("Goalie").PuckToTrack = puck;
        
        puck.FaceoffLocations = GetNode<Node>("Arena/Faceoff Dots");
        
        GameEvents.Instance.RaisePrepareFaceoff(FaceoffDot.CenterIce);
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionPressed("drop_puck"))
        {
            GameEvents.Instance.RaisePrepareFaceoff(FaceoffDot.CenterIce);
        }
    }


    #endregion Overrides

    #region Private Methods

    /// <summary>
    /// When a goal is scored, increase the score, update the ui, freeze the shot visualizer, drop the puck again.
    /// </summary>
    /// <param name="homeGoal"></param>
    /// <returns></returns>
    private void GoalScored(bool homeGoal)
    {
        if (homeGoal)
        {
            _homeScore += 1;
            _homeScoreLbl.Text = $"{_homeScore:00}";
        }
        else
        {
            _awayScore += 1;
            _awayScoreLbl.Text = $"{_awayScore:00}";
        }

        Task.Run(() => AfterGoalTheatrics());
    }

    private async Task AfterGoalTheatrics()
    {
        // TODO: freeze the shot aimer in place, spawn puck after a certain amount of time.
        _shotVisualizer.GoalScored = true;

        await ToSignal(GetTree().CreateTimer(ResetTimer), SceneTreeTimer.SignalName.Timeout);

        GameEvents.Instance.RaisePrepareFaceoff(FaceoffDot.CenterIce);

        _shotVisualizer.GoalScored = false;
        //_spawnedPuck.DropThePuck(FaceoffDot.CenterIce);
    }

    #endregion Private Methods

}