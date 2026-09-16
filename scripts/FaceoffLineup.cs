using System;
using System.Collections.Generic;
using Godot;

namespace BlueLine;

public enum Positions
{
    Center,
    LeftWing,
    RightWing,
    LeftDefense,
    RightDefense
}

/// <summary>
/// Meant to represent the different available faceoff locations.
/// </summary>
public enum FaceoffDot
{
    CenterIce,
    HomeCenter, 
    HomePenInzone,
    HomeBenchInzone,
    HomePenNeutral,
    HomeBenchNeutral,
    AwayCenter,
    AwayPenInzone,
    AwayBenchInzone,
    AwayPenNeutral,
    AwayBenchNeutral,
}

public sealed class FaceoffLineup
{
    private static float forwardsOffsetX = 3.0f;
    private static float wingersOffsetZ = 15.0f;
    private static float defenseOffsetX = 13.0f;
    private static float defenseOffsetZ = 9.0f;
    private static FaceoffLineup _instance;

    public static FaceoffLineup Instance
    { 
        get
        {
            if (_instance == null)
            {
                _instance = new FaceoffLineup();
            }

            return _instance;
        }
    }
    public Dictionary<FaceoffDot, Node3D> FaceoffLocations = [];

    public void SetupFaceoffDots(Node rootFaceoffNode)
    {
        foreach (FaceoffDot faceoffDot in Enum.GetValues<FaceoffDot>())
        {
            Node3D faceoffNode = new();
            switch(faceoffDot)
            {
                case FaceoffDot.CenterIce:
                    faceoffNode = rootFaceoffNode.GetNode<Node3D>("Center Ice");
                    break;
                case FaceoffDot.HomeCenter:
                    faceoffNode = rootFaceoffNode.GetNode<Node3D>("Home Center");
                    break;
                case FaceoffDot.HomePenInzone:
                    faceoffNode = rootFaceoffNode.GetNode<Node3D>("Home Pen Inzone");
                    break;
                case FaceoffDot.HomeBenchInzone:
                    faceoffNode = rootFaceoffNode.GetNode<Node3D>("Home Bench Inzone");
                    break;
                case FaceoffDot.HomePenNeutral:
                    faceoffNode = rootFaceoffNode.GetNode<Node3D>("Home Pen Neutral");
                    break;
                case FaceoffDot.HomeBenchNeutral:
                    faceoffNode = rootFaceoffNode.GetNode<Node3D>("Home Bench Neutral");
                    break;
                case FaceoffDot.AwayCenter:
                    faceoffNode = rootFaceoffNode.GetNode<Node3D>("Away Center");
                    break;
                case FaceoffDot.AwayPenInzone:
                    faceoffNode = rootFaceoffNode.GetNode<Node3D>("Away Pen Inzone");
                    break;
                case FaceoffDot.AwayBenchInzone:
                    faceoffNode = rootFaceoffNode.GetNode<Node3D>("Away Bench Inzone");
                    break;
                case FaceoffDot.AwayPenNeutral:
                    faceoffNode = rootFaceoffNode.GetNode<Node3D>("Away Pen Neutral");
                    break;
                case FaceoffDot.AwayBenchNeutral:
                    faceoffNode = rootFaceoffNode.GetNode<Node3D>("Away Bench Neutral");
                    break;
                default:
                    throw new NotSupportedException($"Faceoff Location: {faceoffDot} is not setup properly. Cannot drop puck");
            }

            FaceoffLocations.Add(faceoffDot, faceoffNode);
        }
    }

    public Vector3 LineupSkater(Positions position, Node3D faceoffDot, bool homeTeam)
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