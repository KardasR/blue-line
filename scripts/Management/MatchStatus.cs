using Godot;
using System.Collections.Generic;

namespace BlueLine.Management;

public partial class MatchStatus : Node
{
    private GameState _state;
    public static MatchStatus Instance { get; private set; }
    public List<PlayerLobbyEntry> ConfirmedPlayers = [];
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


    public override void _EnterTree() => Instance = this;
}