using System;
using System.Collections.Generic;
using System.Linq;

using Godot;

using BlueLine.Management;

namespace BlueLine.Menus;

public partial class TeamSelect : CanvasLayer
{
    #region Members

    private List<PlayerMenuState> _players = [];

    #endregion Members

    #region Enums

    private enum MenuDirection { Left, Right }

    #endregion Enums

    #region Helper Classes

    public class PlayerMenuState
    {
        public int DeviceId { get; set; }
        public string Name { get; set; }
        public SelectedTeam CurrentTeam { get; set; }
        public Positions Position { get; set; }
        public bool IsReady { get; set; }
    }

    /// <summary>
    /// Detroit Red Wing home colors
    /// </summary>
    private struct HomeColors
    {
        public static Color Primary => Color.Color8(206, 17, 38, 255);
        public static Color Secondary => Color.Color8(255, 255, 255, 255);
    }

    /// <summary>
    /// Toronto Maple Leafs away colors
    /// </summary>
    private struct AwayColors
    {
        public static Color Primary => Color.Color8(255, 255, 255, 255);
        public static Color Secondary => Color.Color8(0, 32, 91, 255);
    }

    /// <summary>
    /// Obvious color for debug
    /// </summary>
    private struct UnassignedColors
    {
        public static Color GreenScreen => Color.Color8(4, 244, 4, 255);
    }

    #endregion Helper Classes

    #region Properties

    [Export]
    public PackedScene MainScene;

    [Export]
    public ItemList NoTeam;

    [Export]
    public ItemList HomeTeam;

    [Export]
    public ItemList AwayTeam;

    [Export]
    public Label StatusLabel;

    #endregion Properties

    #region Overrides

    public override void _Ready()
    {
        if (MainScene == null)
        {
            throw new InvalidOperationException("MainScene is null. Can't start game.");
        }
        else if (NoTeam == null)
        {
            throw new InvalidOperationException("Selection ItemList is null. Assign it right goshdarn now you son of a gun.");
        }
        else if (HomeTeam == null)
        {
            throw new InvalidOperationException("Home ItemList is null. Assign it right goshdarn now you son of a gun.");
        }
        else if (AwayTeam == null)
        {
            throw new InvalidOperationException("Away ItemList is null. Assign it now you son of a gun.");
        }
        else if (StatusLabel == null)
        {
            throw new InvalidOperationException("No Status Label was given. Don't be so lazy");
        }

        foreach (int controllerNum in Input.GetConnectedJoypads())
        {
            _players.Add(new() { 
                DeviceId = controllerNum, 
                Name = $"Player {controllerNum}", 
                CurrentTeam = new() { 
                    HomeOrAway=TeamEnum.Unassigned, 
                    PrimaryColor=UnassignedColors.GreenScreen, 
                    SecondaryColor=UnassignedColors.GreenScreen
            } } );
        }

        MatchStatus.Instance.State = GameState.PreGame;
        UpdateTeamLists();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventJoypadMotion || @event is InputEventJoypadButton || @event is InputEventKey)
        {
            int deviceId = @event.Device;
            
            // Find the player matching this controller device ID
            PlayerMenuState player = _players.Find(p => p.DeviceId == deviceId);
            if (player == null) 
                return;

            // Handle ready up decisions
            if (@event.IsActionPressed("ui_accept"))
            {
                // Can only ready up if actually assigned to a team
                if (player.CurrentTeam.HomeOrAway != TeamEnum.Unassigned && !player.IsReady)
                {
                    player.IsReady = true;
                    UpdateTeamLists();
                    CheckStartGameCondition();
                }

                GetViewport()?.SetInputAsHandled();
                return;
            }
            else if (@event.IsActionPressed("ui_cancel"))
            {
                if (player.IsReady)
                {
                    player.IsReady = false;
                    UpdateTeamLists();
                }

                GetViewport()?.SetInputAsHandled();
                return;
            }

            // Lock out movement and position swaps if the player is ready
            if (player.IsReady) 
                return;

            // Handle team and position selection
            if (@event.IsActionPressed("ui_left"))
            {
                ChangePlayerTeam(player, MenuDirection.Left);
                GetViewport()?.SetInputAsHandled(); // Prevent default UI focus shifting
            }
            else if (@event.IsActionPressed("ui_right"))
            {
                ChangePlayerTeam(player, MenuDirection.Right);
                GetViewport()?.SetInputAsHandled();
            }
            else if (@event.IsActionPressed("ui_up"))
            {
                ChangePlayerPosition(player, -1);
                GetViewport()?.SetInputAsHandled();
            }
            else if (@event.IsActionPressed("ui_down"))
            {
                ChangePlayerPosition(player, 1);
                GetViewport()?.SetInputAsHandled();
            }
        }
    }

    #endregion Overrides

    #region Private Methods

    /// <summary>
    /// Allow the 
    /// </summary>
    /// <param name="player"></param>
    /// <param name="direction"></param>
    private void ChangePlayerTeam(PlayerMenuState player, MenuDirection direction)
    {
        if (player.CurrentTeam.HomeOrAway == TeamEnum.Unassigned)
        {
            player.CurrentTeam.HomeOrAway = (direction == MenuDirection.Left) ? TeamEnum.Home : TeamEnum.Away;
        }
        else if (player.CurrentTeam.HomeOrAway == TeamEnum.Home && direction == MenuDirection.Right)
        {
            player.CurrentTeam.HomeOrAway = TeamEnum.Unassigned;
        }
        else if (player.CurrentTeam.HomeOrAway == TeamEnum.Away && direction == MenuDirection.Left)
        {
            player.CurrentTeam.HomeOrAway = TeamEnum.Unassigned;
        }

        // Refresh UI lists to show the new positions
        UpdateTeamLists();
    }

    private void ChangePlayerPosition(PlayerMenuState player, int direction)
    {
        // Unassigned players don't compete for team roles, cycle normally
        if (player.CurrentTeam.HomeOrAway == TeamEnum.Unassigned)
        {
            player.Position = GetNextRawPosition(player.Position, direction);
            UpdateTeamLists();
            return;
        }

        // Team players: cycle past any occupied positions
        Positions checkedPos = player.Position;
        int totalPositions = Enum.GetValues(typeof(Positions)).Length;

        for (int i = 0; i < totalPositions; i++)
        {
            checkedPos = GetNextRawPosition(checkedPos, direction);
            
            // Check if anyone else on the same team is occupying checkedPos
            bool isOccupied = _players.Any(p => p != player && p.CurrentTeam == player.CurrentTeam && p.Position == checkedPos);
            if (!isOccupied)
            {
                player.Position = checkedPos;
                UpdateTeamLists();
                return;
            }
        }
    }

    // Increments/decrements position index and handles wrapping
    private Positions GetNextRawPosition(Positions current, int direction)
    {
        int total = Enum.GetValues(typeof(Positions)).Length;
        int nextIndex = ((int)current + direction) % total;
        if (nextIndex < 0) nextIndex += total;
        return (Positions)nextIndex;
    }

    // Finds a vacant position on a team starting from a preferred option
    private Positions GetFirstAvailablePosition(TeamEnum team, Positions preferred)
    {
        Positions checkedPos = preferred;
        int total = Enum.GetValues(typeof(Positions)).Length;

        for (int i = 0; i < total; i++)
        {
            bool isOccupied = _players.Any(p => p.CurrentTeam.HomeOrAway == team && p.Position == checkedPos);
            if (!isOccupied) return checkedPos;
            checkedPos = GetNextRawPosition(checkedPos, 1);
        }
        return preferred; // Fallback if team is completely full
    }

    private string AbbrevPositionName(Positions pos)
    {
        return pos switch
        {
            Positions.Center => "C",
            Positions.LeftWing => "LW",
            Positions.RightWing => "RW",
            Positions.LeftDefense => "LD",
            Positions.RightDefense => "RD",
            _ => throw new NotImplementedException($"The position, {pos} has not been setup for selection yet.")
        };
    }

    private void UpdateTeamLists()
    {
        // Clear all UI lists
        HomeTeam.Clear();
        NoTeam.Clear();
        AwayTeam.Clear();

        // Populate lists based on team assignment
        foreach (var player in _players)
        {
            string readyStatus = player.IsReady ? " [READY]" : "";
            string displayName = $"{player.Name} [{AbbrevPositionName(player.Position)}]{readyStatus}";

            switch (player.CurrentTeam.HomeOrAway)
            {
                case TeamEnum.Home:
                    HomeTeam.AddItem(displayName);
                    player.CurrentTeam.PrimaryColor = HomeColors.Primary;
                    player.CurrentTeam.SecondaryColor = HomeColors.Secondary;
                    break;
                case TeamEnum.Unassigned:
                    NoTeam.AddItem(displayName);
                    player.CurrentTeam.PrimaryColor = UnassignedColors.GreenScreen;
                    player.CurrentTeam.SecondaryColor = UnassignedColors.GreenScreen;
                    break;
                case TeamEnum.Away:
                    AwayTeam.AddItem(displayName);
                    player.CurrentTeam.PrimaryColor = AwayColors.Primary;
                    player.CurrentTeam.SecondaryColor = AwayColors.Secondary;
                    break;
            }
        }
    }

    private void CheckStartGameCondition()
    {
        bool allAssigned = _players.All(p => p.CurrentTeam.HomeOrAway != TeamEnum.Unassigned);
        bool allReady = _players.All(p => p.IsReady);

        if (allAssigned && allReady)
        {
            if (StatusLabel != null) StatusLabel.Text = "Starting game...";
            
            ConfirmAndLoadMatch();
        }
    }

    private void ConfirmAndLoadMatch()
    {
        MatchStatus.Instance.PlayersToSpawn.Clear();

        MatchStatus.Instance.PlayersToSpawn.AddRange(BuildSpawnConfigs(10));

        GetTree().ChangeSceneToPacked(MainScene);
    }

    private List<PlayerSpawnConfig> BuildSpawnConfigs(int numToSpawn)
    {
        List<PlayerSpawnConfig> list = [];

        // TODO: 1v1 will only spawn centers. This will cause issues if a user doesn't select the center position
        for(int spawnCount = 0; spawnCount < numToSpawn; spawnCount++)
        {
            bool isHomeTeam = spawnCount % 2 == 0;

            PlayerSpawnConfig skater = new()
            {
                PlayerTeam = new() { 
                    HomeOrAway      = isHomeTeam ? TeamEnum.Home : TeamEnum.Away,
                    PrimaryColor    = isHomeTeam ? HomeColors.Primary : AwayColors.Primary,
                    SecondaryColor  = isHomeTeam ? HomeColors.Secondary : AwayColors.Secondary },
                PlayerId = spawnCount,
                DeviceId = -1,
                Assignment = GetPlayerPosition(spawnCount),
                SkinColor = Color.Color8(241, 194, 125, 255),
            };

            list.Add(skater);
        }

        foreach (PlayerMenuState player in _players)
        {
            PlayerSpawnConfig config = list.Find(
                f => f.PlayerTeam.HomeOrAway == player.CurrentTeam.HomeOrAway && f.Assignment == player.Position
            );

            if (!config.Equals(default(PlayerSpawnConfig)))
            {
                list[config.PlayerId] = new() { 
                    Assignment=config.Assignment, 
                    DeviceId=player.DeviceId,
                    PlayerId=config.PlayerId,
                    PlayerTeam=config.PlayerTeam,
                    SkinColor=config.SkinColor,
                };
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