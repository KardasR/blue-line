using Godot;

using System;
using System.Collections.Generic;

namespace BlueLine.VideoFeed;

public enum CameraMode { SplitScreen, FollowFixed }

public partial class CameraManager : Node
{
    private ICameraRig _activeRig;

    public static CameraManager Instance { get; private set; }

    [Export]
    public PackedScene FollowCameraScene { get; set; }

    [Export]
    public Control SplitScreenControl { get; set; }

    public event Action RigChanged;

    public override void _EnterTree() => Instance = this;

    public void SetMode(CameraMode mode, IReadOnlyList<Node3D> players, Node3D puck)
    {
        if (FollowCameraScene == null)
        {
            throw new InvalidOperationException("No follow camera scene was given. Cannot make a follow camera mode.");
        }
        if (SplitScreenControl == null)
        {
            throw new InvalidOperationException("No split screen control was given. Cannot make a split screen camera mode.");
        }

        _activeRig?.Teardown();
        _activeRig = mode switch
        {
            CameraMode.SplitScreen   => new SplitScreenCameraRig(FollowCameraScene, SplitScreenControl),
            CameraMode.FollowFixed   => new FollowFixedCameraRig(FollowCameraScene, this),
            _ => throw new NotSupportedException(nameof(mode))
        };
        _activeRig.Setup(players, puck);
        RigChanged?.Invoke();
    }

    public Camera3D GetCameraForPlayer(int playerIndex) => _activeRig?.GetCameraForPlayer(playerIndex);

    public override void _PhysicsProcess(double delta) => _activeRig?.Tick(delta);
}