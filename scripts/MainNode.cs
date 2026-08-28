using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Godot;

using BlueLine.Goaltender;
using BlueLine.FrozenRubber;
using BlueLine.VideoFeed;
using BlueLine.Skater;

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

    private readonly List<Hazmat> _players = new();

    #endregion Members

    #region Structs

    public struct PlayerSpawnConfig
    {
        public int PlayerId;
        //public Team Team;
        public bool HomeTeam;
        public int DeviceId;
        public Vector3 SpawnPosition;
        public PlayerAttributes Attributes;
    }

    #endregion Structs

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
    /// Home Goalie.
    /// </summary>
    [Export]
    public Goalie HomeGoalie { get; set; }

    /// <summary>
    /// Away Goal.
    /// </summary>
    [Export]
    public Net AwayNet { get; set; }

    /// <summary>
    /// Away Goalie.
    /// </summary>
    [Export]
    public Goalie AwayGoalie { get; set; }

    /// <summary>
    /// How long until the game respawns the puck after a goal.
    /// </summary>
    [Export]
    public float ResetTimer { get; set; } = 3.0f;

    /// <summary>
    /// A scene used for spawning a player.
    /// </summary>
    [Export] 
    public PackedScene PlayerScene;

    /// <summary>
    /// A list of players that were spawned into the scene.
    /// </summary>
    public IReadOnlyList<Hazmat> Players => _players;

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
        if (HomeGoalie == null)
        {
            throw new InvalidOperationException("Home Goalie was not setup.");
        }
        if (AwayNet == null)
        {
            throw new InvalidOperationException("Away Goal was not setup. Cannot react to a goal.");
        }
        if (AwayGoalie == null)
        {
            throw new InvalidOperationException("Away Goalie was not setup.");
        }

        // setup refs
        _homeScoreLbl = GetNode<Label>("UI/Score Board/Home Score");
        _awayScoreLbl = GetNode<Label>("UI/Score Board/Away Score");
        _shotVisualizer = GetNode<ShotVisualizer>("Shot Visualizer");

        // subscribe to events
        GameEvents.Instance.GoalScored += On_GoalScored;

        // create and add the puck and players to the scene
        Puck puck = PuckScene.Instantiate<Puck>();
        _spawnedPuck = puck;

        foreach (PlayerSpawnConfig config in BuildSpawnConfigs())
        {
            Hazmat player = PlayerScene.Instantiate<Hazmat>();
            player.HomeTeam = config.HomeTeam;
            player.DeviceId = config.DeviceId;
            player.Attributes = config.Attributes;
            player.PlayerId = config.PlayerId;

            player.AttackingGoal = config.HomeTeam ? AwayNet : HomeNet;

            AddChild(player);

            player.GlobalPosition = config.SpawnPosition; // set after AddChild so it's not overwritten by scene defaults
            player.GlobalRotation = new() { 
                X = player.GlobalRotation.X, 
                Y = config.HomeTeam ? -Mathf.Pi/2 : Mathf.Pi/2, 
                Z = player.GlobalRotation.Z 
            };

            _players.Add(player);
        }

        CameraManager.Instance.SetMode(
            CameraMode.FollowFixed,
            _players.Cast<Node3D>().ToList(),
            puck
        );

        AddChild(puck);

        HomeGoalie.PuckToTrack = puck;
        HomeGoalie.GoalToDefend = HomeNet;

        AwayGoalie.PuckToTrack = puck;
        AwayGoalie.GoalToDefend = AwayNet;
        
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
    }

    private List<PlayerSpawnConfig> BuildSpawnConfigs()
    {
        List<PlayerSpawnConfig> list = new();

        PlayerSpawnConfig player1 = new();

        player1.HomeTeam = true;
        player1.DeviceId = 0;
        player1.PlayerId = 0;
        player1.SpawnPosition = FaceoffLineup.LineupSkater(Positions.Center, GetNode<Node3D>("Arena/Faceoff Dots/Center Ice"), true);
        player1.Attributes = new();

        list.Add(player1);

        PlayerSpawnConfig player2 = new();

        player2.HomeTeam = false;
        player2.DeviceId = 0;
        player2.PlayerId = 0;
        player2.SpawnPosition = FaceoffLineup.LineupSkater(Positions.Center, GetNode<Node3D>("Arena/Faceoff Dots/Center Ice"), false);
        player2.Attributes = new();

        list.Add(player2);

        return list;
    }

    #endregion Private Methods

}