using System;
using Godot;

namespace BlueLine;

public enum Positions
{
    Center,
    LeftWing,
    RightWing,
    LeftDefense,
    RightDefense,
    Goalie
}

public static class FaceoffLineup
{
    private static float forwardsOffsetX = 3.0f;
    private static float wingersOffsetZ = 15.0f;
    private static float defenseOffsetX = 13.0f;
    private static float defenseOffsetZ = 9.0f;

    public static Vector3 LineupSkater(Positions position, Node3D faceoffDot, bool homeTeam)
    {
        return position switch
        {
            Positions.Center => new() 
            { 
                X = faceoffDot.GlobalPosition.X + (homeTeam ? forwardsOffsetX : -forwardsOffsetX), 
                Y = 0, 
                Z = 0
            },
            Positions.LeftWing => new() 
            { 
                X = faceoffDot.GlobalPosition.X + (homeTeam ? forwardsOffsetX : -forwardsOffsetX), 
                Y = 0, 
                Z = faceoffDot.GlobalPosition.Z + (homeTeam ? wingersOffsetZ : -wingersOffsetZ)
            },
            Positions.RightWing => new() 
            { 
                X = faceoffDot.GlobalPosition.X + (homeTeam ? forwardsOffsetX : -forwardsOffsetX), 
                Y = 0, 
                Z = faceoffDot.GlobalPosition.Z + (homeTeam ? -wingersOffsetZ : wingersOffsetZ)
            },
            Positions.LeftDefense => new()
            {
                X = faceoffDot.GlobalPosition.X + (homeTeam ? defenseOffsetX : -defenseOffsetX), 
                Y = 0, 
                Z = faceoffDot.GlobalPosition.Z + (homeTeam ? defenseOffsetZ : -defenseOffsetZ)
            },
            Positions.RightDefense => new()
            {
                X = faceoffDot.GlobalPosition.X + (homeTeam ? defenseOffsetX : -defenseOffsetX), 
                Y = 0, 
                Z = faceoffDot.GlobalPosition.Z + (homeTeam ? -defenseOffsetZ : defenseOffsetZ)
            },
            _ => throw new NotSupportedException("What in tarnation.")
        };
    }
}