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
    Paused
}

public struct PlayerSpawnConfig
{
    public int PlayerId;
    public int DeviceId;
    public bool HomeTeam;
    public Positions Assignment;
    public Vector3 SpawnPosition;
    public Vector3 SpawnRotation;
}

public struct PlayerLobbyEntry
{
    public int DeviceId;
    public bool HomeTeam;
    public Positions Position;
}