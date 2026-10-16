using BlueLine.Skaters;
using Godot;

namespace BlueLine.Management;

public enum GameState
{
    MainMenu,
    PreGame,
    Faceoff,
    Playing,
    Goal,
    GameOver,
    Paused,
    EndOfPeriod
}

public enum TeamEnum { Home, Unassigned, Away }

public struct PlayerSpawnConfig
{
    public int PlayerId;
    public int DeviceId;
    public SelectedTeam PlayerTeam;
    public Positions Assignment;
    public Color SkinColor;
}

public struct PlayerInfo
{
    public int DeviceId;
    public bool IsHomeTeam;
}

public class SelectedTeam
{
    /// <summary>
    /// Is the team the home or away team?
    /// </summary>
    public TeamEnum HomeOrAway { get; set; }

    /// <summary>
    /// Main color of the jersey and equipment.
    /// </summary>
    public Color PrimaryColor { get; set; }

    /// <summary>
    /// Color of the stripes.
    /// </summary>
    public Color SecondaryColor { get; set; }
}