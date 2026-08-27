using Godot;
using System;

namespace BlueLine.Management;

public partial class GameEvents : Node
{
    public static GameEvents Instance { get; private set; }

    public event Action<Vector3, float> ShotFired;
    public event Action PuckSaved;
    public event Action<bool> GoalScored;
    public event Action<FaceoffDot> PrepareFaceoff;
    public event Action<FaceoffDot> PuckDropped;

    public override void _EnterTree()
    {
        Instance = this;
    }

    public void RaiseShotFired(Vector3 direction, float force) => ShotFired?.Invoke(direction, force);
    public void RaisePuckSaved() => PuckSaved?.Invoke();
    public void RaiseGoalScored(bool homeGoal) => GoalScored?.Invoke(homeGoal);
    public void RaisePrepareFaceoff(FaceoffDot dot) => PrepareFaceoff?.Invoke(dot);
    public void RaisePuckDropped(FaceoffDot dot) => PuckDropped?.Invoke(dot);
}
