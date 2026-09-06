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

    private ShotVisualizer _homeShotVisualizer;

    private ShotVisualizer _awayShotVisualizer;

    private readonly List<Hazmat> _players = new();

    #endregion Members

    #region Structs



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
    /// 
    /// </summary>
    [Export]
    public PackedScene InputScene;

    /// <summary>
    /// A list of players that were spawned into the scene.
    /// </summary>
    public IReadOnlyList<Hazmat> Players => _players;

    public GameState CurrentGameState { get; private set; }

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


    public void On_ControllerConnectionChanged(long deviceID, bool connected)
    {
        // TODO: handle dynamically responding to controller plugins/unplugs
        // search the players list for deviceID and assign/unassign ControllerInput
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
        _homeShotVisualizer = GetNode<ShotVisualizer>("Home Shot Visualizer");
        _awayShotVisualizer = GetNode<ShotVisualizer>("Away Shot Visualizer");

        // subscribe to events
        GameEvents.Instance.GoalScored += On_GoalScored;
        Input.JoyConnectionChanged += On_ControllerConnectionChanged;

        SpawnAndSetupGame();
        
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

    private void SpawnAndSetupGame()
    {
        // spawn and setup controller inputs
        int numOfContr = Input.GetConnectedJoypads().Count;
        for (int i = 0; i < numOfContr; i++)
        {
            //TODO: make a state machine to handle disconnects and new controllers.
            ControllerInput playerInput = InputScene.Instantiate<ControllerInput>();
            playerInput.Name = $"ControllerInput{i}";
            playerInput.DeviceId = i;

            AddChild(playerInput);
        }

        // create and add the puck and players to the scene
        Puck puck = PuckScene.Instantiate<Puck>();
        _spawnedPuck = puck;

        // int SkaterCount = Input.GetConnectedJoypads().Count > 1 ? Input.GetConnectedJoypads().Count : 2;    // for now make sure there's two skaters
        // SkaterCount = 10;
        foreach (PlayerSpawnConfig config in BuildSpawnConfigs(MatchStatus.Instance.ConfirmedPlayers.Count, true))
        {
            Hazmat player = PlayerScene.Instantiate<Hazmat>();
            player.Name = $"Skater-{config.PlayerId}";
            player.HomeTeam = config.HomeTeam;
            player.PlayerId = config.PlayerId;
            player.AttackingGoal = config.HomeTeam ? AwayNet : HomeNet;
            player.Assignment = config.Assignment;

            ControllerInput node = config.DeviceId != -1 ? node = GetNode<ControllerInput>($"ControllerInput{config.DeviceId}") : null;

            if (numOfContr > 0 && config.HomeTeam)
            {
                player.InputDevice = node;
                _homeShotVisualizer.Controller = _homeShotVisualizer.Controller == null ? node : null;
                numOfContr -= 1;
            }
            else if (numOfContr > 0 && !config.HomeTeam)
            {
                player.InputDevice = node;
                _awayShotVisualizer.Controller = _awayShotVisualizer.Controller == null ? node : null;
                numOfContr -= 1;
            }

            AddChild(player);

            player.GlobalPosition = config.SpawnPosition; // set after AddChild so it's not overwritten by scene defaults
            player.LookAt(GetNode<Node3D>("Arena/Faceoff Dots/Center Ice").GlobalPosition, useModelFront: true);
            player.GlobalRotation = new() {
                X = 0, 
                Y = player.GlobalRotation.Y,
                Z = player.GlobalRotation.Z
            };

            _players.Add(player);
        }

        // setup the teammates for each player.
        foreach(Hazmat skater in _players)
        {
            skater.Teammates = _players.Where(s => s != skater && s.HomeTeam == skater.HomeTeam).ToList();
        }

        CameraManager.Instance.SetMode(
            Input.GetConnectedJoypads().Count <= 1 ? CameraMode.FollowFixed : CameraMode.SplitScreen,
            _players,
            puck
        );

        AddChild(puck);

        HomeGoalie.PuckToTrack = puck;
        HomeGoalie.GoalToDefend = HomeNet;

        AwayGoalie.PuckToTrack = puck;
        AwayGoalie.GoalToDefend = AwayNet;
        
        puck.FaceoffLocations = GetNode<Node>("Arena/Faceoff Dots");
    }

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
        await ToSignal(GetTree().CreateTimer(ResetTimer), SceneTreeTimer.SignalName.Timeout);

        GameEvents.Instance.RaisePrepareFaceoff(FaceoffDot.CenterIce);
    }

    private List<PlayerSpawnConfig> BuildSpawnConfigs(int numToSpawn, bool pvp)
    {
        List<PlayerSpawnConfig> list = [];
        
        if (pvp)
        {
            for(int spawnCount = 0; spawnCount < MatchStatus.Instance.ConfirmedPlayers.Count; spawnCount++)
            {
                PlayerSpawnConfig skater = new()
                {
                    HomeTeam = MatchStatus.Instance.ConfirmedPlayers[spawnCount].HomeTeam,
                    PlayerId = spawnCount,
                    DeviceId = MatchStatus.Instance.ConfirmedPlayers[spawnCount].DeviceId,
                    Assignment = MatchStatus.Instance.ConfirmedPlayers[spawnCount].Position,                    
                    SpawnPosition = FaceoffLineup.LineupSkater(GetPlayerPosition(spawnCount), GetNode<Node3D>("Arena/Faceoff Dots/Center Ice"), MatchStatus.Instance.ConfirmedPlayers[spawnCount].HomeTeam)
                };

                list.Add(skater);
            }
        }
        else
        {
            for(int spawnCount = 0; spawnCount < numToSpawn; spawnCount++)
            {
                PlayerSpawnConfig skater = new()
                {
                    HomeTeam = spawnCount % 2 == 0,
                    PlayerId = spawnCount,
                    DeviceId = Input.GetConnectedJoypads().Count > spawnCount ? spawnCount : -1,
                    Assignment = GetPlayerPosition(spawnCount),
                    SpawnPosition = FaceoffLineup.LineupSkater(GetPlayerPosition(spawnCount), GetNode<Node3D>("Arena/Faceoff Dots/Center Ice"), spawnCount % 2 == 0)
                };

                list.Add(skater);
            }
        }

        return list;

        static Positions GetPlayerPosition(int playerID)
        {
            if (playerID % 2 != 0)
            {
                playerID -= 1;
            }

            return (Positions)(playerID / 2);
        }
    }

    #endregion Private Methods

}