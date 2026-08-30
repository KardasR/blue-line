using BlueLine.Skater;
using Godot;

using System;
using System.Collections.Generic;

namespace BlueLine.VideoFeed;

public class FollowFixedCameraRig : ICameraRig
{
    private FollowCamera _rig;
    private Camera3D _camera;

    private PackedScene _followCameraScene;
    private Node _parent;

    public FollowFixedCameraRig(PackedScene cameraScene, Node parent)
    {
        _followCameraScene = cameraScene;
        _parent = parent;
    }

    public void Setup(IReadOnlyList<Hazmat> players, Node3D puck)
    {
        if (_followCameraScene == null)
        {
            throw new InvalidOperationException("No follow camera scene was given. Cannot setup follow camera");
        }
        if (_parent == null)
        {
            throw new InvalidOperationException("No parent was given. The camera won't be able to show anything.");
        }

        _rig = _followCameraScene.Instantiate<FollowCamera>();
        _parent.AddChild(_rig);

        _camera = _rig.GetNode<Camera3D>("Camera Pos/Camera");
        _rig.Target = players[0];   // default to the first created skater being the main player.

        _camera.Current = true;
    }

    public Camera3D GetCameraForPlayer(int playerIndex) => _camera;

    public void Tick(double delta) { } // FollowCamera drives itself via _Process

    public void Teardown() => _rig?.QueueFree();
}