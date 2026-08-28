using System;
using Godot;

namespace BlueLine;

public enum Positions
{
    Center,
    LeftWing,
    RightWing,
    LeftDefence,
    RightDefence,
    Goalie
}

public static class FaceoffLineup
{
    public static Vector3 LineupSkater(Positions position, Node3D faceoffDot, bool homeTeam)
    {
        float forwardsOffsetX = homeTeam ? 3f : -3f;

        return position switch
        {
            Positions.Center => new() { X = faceoffDot.GlobalPosition.X + forwardsOffsetX, Y = 0, Z = 0 },
            _ => throw new NotSupportedException("I haven't setup non-centers yet.")
        };
    }
}