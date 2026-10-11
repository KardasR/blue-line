using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using Godot;

using BlueLine.Goaltender;
using BlueLine.FrozenRubber;
using BlueLine.VideoFeed;
using BlueLine.Skaters;

namespace BlueLine.Management;

public partial class MainNode : Node
{
    #region Members

    private ShotVisualizer _homeShotVisualizer;

    private ShotVisualizer _awayShotVisualizer;

    private readonly List<Skater> _players = [];

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
    /// Scene used for holding the controller input scripts for each controller.
    /// </summary>
    [Export]
    public PackedScene InputScene;

    /// <summary>
    /// Scene used for holding the ai skater scripts for each skater.
    /// </summary>
    [Export]
    public PackedScene AiSkaterScene;

    /// <summary>
    /// A list of players that were spawned into the scene.
    /// </summary>
    public IReadOnlyList<Skater> Players => _players;

    #endregion Properties

    #region Events

    /// <summary>
    /// React to a goal being scored.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public void On_GoalScored(bool _)
    {
        Task.Run(() => AfterGoalTheatrics());
    }

    /// <summary>
    /// This will allow the scene to react to changes of the game state (pause, end of period, etc)
    /// </summary>
    /// <param name="state"></param>
    public void On_GameState_Changed(GameState state)
    {
        // TODO: Add Pause menu

        switch (state)
        {
            default:
                break;
        }
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
        if (InputScene == null)
        {
            throw new InvalidOperationException("No input scene was given. Cannot respond to inputs.");
        }
        if (PlayerScene == null)
        {
            throw new InvalidOperationException("No player scene was given. Cannot spawn players.");
        }

        // setup refs
        _homeShotVisualizer = GetNode<ShotVisualizer>("Home Shot Visualizer");
        _awayShotVisualizer = GetNode<ShotVisualizer>("Away Shot Visualizer");

        // subscribe to events
        GameEvents.Instance.GoalScored += On_GoalScored;
        GameEvents.Instance.ChangeGameState += On_GameState_Changed;
        GameEvents.Instance.PrepareFaceoff += SetupPlayersForFaceoff;

        SpawnAndSetupGame();
        
        GameEvents.Instance.RaisePrepareFaceoff(FaceoffDot.CenterIce);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventJoypadButton button)
        {
            if (button.IsActionPressed("drop_puck"))
                GameEvents.Instance.RaisePrepareFaceoff(FaceoffDot.CenterIce);
            else if (button.IsActionPressed("start"))
                GameEvents.Instance.RaiseChangeGameState(GameState.Paused);
        }
    }

    #endregion Overrides

    #region Private Methods

    private void SpawnAndSetupGame()
    {
        // spawn and setup controller inputs
        int numOfUnassContr = Input.GetConnectedJoypads().Count;
        for (int i = 0; i < numOfUnassContr; i++)
        {
            //TODO: make a state machine to handle disconnects and new controllers.
            ControllerInput playerInput = InputScene.Instantiate<ControllerInput>();
            playerInput.Name = $"ControllerInput{i}";
            playerInput.DeviceId = i;

            AddChild(playerInput);
        }

        // Setup faceoff dot refs
        FaceoffLineup.Instance.SetupFaceoffDots(GetNode<Node>("Arena/Faceoff Dots"));

        // create and add the puck and players to the scene
        Puck puck = PuckScene.Instantiate<Puck>();
        MatchStatus.Instance.SpawnedPuck = puck;
        
        foreach (PlayerSpawnConfig config in MatchStatus.Instance.ConfirmedPlayers)
        {
            Skater player = PlayerScene.Instantiate<Skater>();
            
            player.Name = $"Skater-{config.PlayerId}";
            player.IsHomeTeam = config.PlayerTeam.HomeOrAway == TeamEnum.Home;
            player.PlayerId = config.PlayerId;
            player.AttackingGoal = player.IsHomeTeam ? AwayNet : HomeNet;
            player.Assignment = config.Assignment;
            player.PrimaryColor = config.PlayerTeam.PrimaryColor;
            player.SecondaryColor = config.PlayerTeam.SecondaryColor;
            player.SkinColor = config.SkinColor;

            BaseInput input = config.DeviceId != -1 ? GetNode<ControllerInput>($"ControllerInput{config.DeviceId}") : new AiInput() { Name = $"Ai-{config.PlayerId}"};

            player.InputDevice = input;

            AiSkater ai = config.DeviceId == -1 ? AiSkaterScene.Instantiate<AiSkater>() : null;

            if (_homeShotVisualizer.Controller == null &&
                numOfUnassContr > 0 && 
                player.IsHomeTeam && 
                input is ControllerInput cont)
            {
                _homeShotVisualizer.Controller = cont;
                numOfUnassContr -= 1;
            }
            else if (_awayShotVisualizer.Controller == null &&
                    numOfUnassContr > 0 && 
                    !player.IsHomeTeam &&
                    input is ControllerInput cont2)
            {
                _awayShotVisualizer.Controller = cont2;
                numOfUnassContr -= 1;
            }

            if (ai != null)
                player.AddChild(ai);

            AddChild(player);
            
            player.GlobalPosition = FaceoffLineup.Instance.LineupSkater(
                GetPlayerPosition(config.PlayerId), 
                FaceoffLineup.Instance.FaceoffLocations[FaceoffDot.CenterIce], 
                config.PlayerTeam.HomeOrAway == TeamEnum.Home
            ); // set after AddChild so it's not overwritten by scene defaults

            player.LookAt(GetNode<Node3D>("Arena/Faceoff Dots/Center Ice").GlobalPosition, useModelFront: true);
            player.GlobalRotation = new() {
                X = 0, 
                Y = player.GlobalRotation.Y,
                Z = player.GlobalRotation.Z
            };

            _players.Add(player);
        }

        // setup the teammates for each player.
        foreach(Skater skater in _players)
        {
            skater.Teammates = _players.Where(s => s != skater && s.IsHomeTeam == skater.IsHomeTeam).ToList();
        }

        CameraManager.Instance.SetMode(
            Input.GetConnectedJoypads().Count <= 1 ? CameraMode.FollowFixed : CameraMode.SplitScreen,
            _players
        );

        AddChild(puck);

        HomeGoalie.GoalToDefend = HomeNet;

        AwayGoalie.GoalToDefend = AwayNet;

        static Positions GetPlayerPosition(int playerID)
        {
            if (playerID % 2 != 0)
            {
                playerID -= 1;
            }

            return (Positions)(playerID / 2);
        }
    }

    private async Task AfterGoalTheatrics()
    {
        await ToSignal(GetTree().CreateTimer(ResetTimer), SceneTreeTimer.SignalName.Timeout);

        GameEvents.Instance.RaisePrepareFaceoff(FaceoffDot.CenterIce);
    }

    private void SetupPlayersForFaceoff(FaceoffDot dot)
    {
        // we want to loop through the Players list and spawn each player according to where they should be.
        foreach (Skater skater in Players)
        {
            skater.Velocity = Vector3.Zero;
            skater.GlobalPosition = FaceoffLineup.Instance.LineupSkater(skater.Assignment, FaceoffLineup.Instance.FaceoffLocations[dot], skater.IsHomeTeam);
            skater.LookAt(FaceoffLineup.Instance.FaceoffLocations[dot].GlobalPosition, useModelFront:true);
            skater.GlobalRotation = new() { 
                X=0, 
                Y=skater.GlobalRotation.Y, 
                Z=skater.GlobalRotation.Z 
            };
        }
    }

    #endregion Private Methods

}