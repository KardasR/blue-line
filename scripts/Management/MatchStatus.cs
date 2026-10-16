using Godot;
using System.Collections.Generic;

using BlueLine.FrozenRubber;
using BlueLine.Skaters;

namespace BlueLine.Management;

public partial class MatchStatus : Node
{
    private GameState _state;
    public static MatchStatus Instance { get; private set; }
    public List<PlayerSpawnConfig> PlayersToSpawn = [];
    public GameState State 
    { 
        get
        {
            return _state;
        }
        set
        {
            if (_state != value)
            {
                _state = value;
                GameEvents.Instance.RaiseChangeGameState(value);
            }
        } 
    }

    public Puck SpawnedPuck { get; set; }

    public IReadOnlyList<Skater> SpawnedSkaters { get; set; }

    public IReadOnlyList<PlayerInfo> PlayerControllers { get; set; }

    public override void _EnterTree() => Instance = this;
}