using Godot;
using System.Collections.Generic;
using System.Dynamic;

namespace BlueLine.Management;

public partial class MatchStatus : Node
{
    public static MatchStatus Instance { get; private set; }
    public List<PlayerLobbyEntry> ConfirmedPlayers = [];
    public GameState State { get; set; }


    public override void _EnterTree() => Instance = this;
}