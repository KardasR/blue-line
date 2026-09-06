using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace BlueLine.Management;

public partial class TeamSelect : CanvasLayer
{
    #region Members

    private List<PlayerState> _players = [];

    #endregion Members

    #region Enums

    public enum SelectedTeam { Home, Unassigned, Away }
    public enum MenuDirection { Left, Right }

    #endregion Enums

    #region Helper Classes

    public class PlayerState
    {
        public int DeviceId { get; set; }
        public string Name { get; set; }
        public SelectedTeam CurrentTeam { get; set; }
        public Positions Position { get; set; }
        public bool IsReady { get; set; }
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
            throw new InvalidOperationException("Middle ItemList is null. Assign it right goshdarn now you son of a gun.");
        }
        else if (HomeTeam == null)
        {
            throw new InvalidOperationException("Middle ItemList is null. Assign it right goshdarn now you son of a gun.");
        }
        else if (StatusLabel == null)
        {
            throw new InvalidOperationException("No Status Label was given. Don't be so lazy");
        }

        foreach (int controllerNum in Input.GetConnectedJoypads())
        {
            _players.Add(new() { DeviceId = controllerNum, Name = $"Player {controllerNum}", CurrentTeam = SelectedTeam.Unassigned });
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
            PlayerState player = _players.Find(p => p.DeviceId == deviceId);
            if (player == null) 
                return;

            // Handle ready up decisions
            if (@event.IsActionPressed("ui_accept"))
            {
                // Can only ready up if actually assigned to a team
                if (player.CurrentTeam != SelectedTeam.Unassigned && !player.IsReady)
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
                MovePlayer(player, MenuDirection.Left);
                GetViewport()?.SetInputAsHandled(); // Prevent default UI focus shifting
            }
            else if (@event.IsActionPressed("ui_right"))
            {
                MovePlayer(player, MenuDirection.Right);
                GetViewport()?.SetInputAsHandled();
            }
            else if (@event.IsActionPressed("ui_up"))
            {
                CyclePlayerPosition(player, -1);
                GetViewport()?.SetInputAsHandled();
            }
            else if (@event.IsActionPressed("ui_down"))
            {
                CyclePlayerPosition(player, 1);
                GetViewport()?.SetInputAsHandled();
            }
        }
    }

    #endregion Overrides

    #region Private Methods

    private void  MovePlayer(PlayerState player, MenuDirection direction)
    {
        if (player.CurrentTeam == SelectedTeam.Unassigned)
        {
            player.CurrentTeam = (direction == MenuDirection.Left) ? SelectedTeam.Home : SelectedTeam.Away;
        }
        else if (player.CurrentTeam == SelectedTeam.Home && direction == MenuDirection.Right)
        {
            player.CurrentTeam = SelectedTeam.Unassigned;
        }
        else if (player.CurrentTeam == SelectedTeam.Away && direction == MenuDirection.Left)
        {
            player.CurrentTeam = SelectedTeam.Unassigned;
        }

        // Refresh UI lists to show the new positions
        UpdateTeamLists();
    }

    private void CyclePlayerPosition(PlayerState player, int direction)
    {
        // Unassigned players don't compete for team roles, cycle normally
        if (player.CurrentTeam == SelectedTeam.Unassigned)
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
    private Positions GetFirstAvailablePosition(SelectedTeam team, Positions preferred)
    {
        Positions checkedPos = preferred;
        int total = Enum.GetValues(typeof(Positions)).Length;

        for (int i = 0; i < total; i++)
        {
            bool isOccupied = _players.Any(p => p.CurrentTeam == team && p.Position == checkedPos);
            if (!isOccupied) return checkedPos;
            checkedPos = GetNextRawPosition(checkedPos, 1);
        }
        return preferred; // Fallback if team is completely full
    }

    private string FormatPositionName(Positions pos)
    {
        return pos switch
        {
            Positions.Center => "C",
            Positions.LeftWing => "LW",
            Positions.RightWing => "RW",
            Positions.LeftDefense => "LD",
            Positions.RightDefense => "RD",
            _ => pos.ToString()
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
            string displayName = $"{player.Name} [{FormatPositionName(player.Position)}]{readyStatus}";

            switch (player.CurrentTeam)
            {
                case SelectedTeam.Home:
                    HomeTeam.AddItem(displayName);
                    break;
                case SelectedTeam.Unassigned:
                    NoTeam.AddItem(displayName);
                    break;
                case SelectedTeam.Away:
                    AwayTeam.AddItem(displayName);
                    break;
            }
        }
    }

    private void CheckStartGameCondition()
    {
        bool allAssigned = _players.All(p => p.CurrentTeam != SelectedTeam.Unassigned);
        bool allReady = _players.All(p => p.IsReady);

        if (allAssigned && allReady)
        {
            if (StatusLabel != null) StatusLabel.Text = "Starting game...";
            
            ConfirmAndLoadMatch();
        }
    }

    private void ConfirmAndLoadMatch()
    {
        MatchStatus.Instance.ConfirmedPlayers.Clear();

        foreach (PlayerState player in _players)
        {
            MatchStatus.Instance.ConfirmedPlayers.Add(new() 
            { 
                DeviceId = player.DeviceId, 
                HomeTeam = player.CurrentTeam == SelectedTeam.Home, 
                Position = player.Position 
            });
        }

        GetTree().ChangeSceneToPacked(MainScene);
    }

    #endregion Private Methods
}