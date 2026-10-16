using BlueLine.Management;
using BlueLine.Skaters;

using Godot;

using System;
using System.Collections.Generic;
using System.Linq;

namespace BlueLine.VideoFeed;

public class FollowFixedCameraRig : ICameraRig
{
    private FollowCamera _rig;
    private Camera3D _camera;
    private bool _isFollowingHomeTeam;

    private PackedScene _followCameraScene;
    private Node _parent;

    public FollowFixedCameraRig(PackedScene cameraScene, Node parent)
    {
        _followCameraScene = cameraScene;
        _parent = parent;

        GameEvents.Instance.NewPuckCarrier += ReactToNewPuckCarrier;
    }

    public void Setup(IReadOnlyList<Skater> players)
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
        Skater sk8r = players.First(p => p.InputDevice?.DeviceId == MatchStatus.Instance.PlayersToSpawn[0].DeviceId);
        _rig.Target = sk8r;
        _isFollowingHomeTeam = sk8r.IsHomeTeam;

        _camera.Current = true;

        if (!sk8r.IsHomeTeam)
            _rig.RotateY(Mathf.Pi);
    }

    private void ReactToNewPuckCarrier(Skater skater)
    {
        if (_isFollowingHomeTeam == skater.IsHomeTeam)
        {
            _rig.Target = skater;
        }
    }

    public Camera3D GetCameraForPlayer(int playerIndex) => _camera;

    public void Tick(double delta) { } // FollowCamera drives itself via _Process

    public void Teardown() => _rig?.QueueFree();
}