using BlueLine.Skater;
using BlueLine.Management;
using Godot;

using System;
using System.Collections.Generic;
using System.Linq;

namespace BlueLine.VideoFeed;

public class SplitScreenCameraRig : ICameraRig
{
    private PackedScene _followCameraScene;
    private Control _uiParent;
    private List<SubViewportContainer> _containers = [];
    private List<FollowCamera> _rigs = [];
    private List<Camera3D> _cameras = [];
    private IReadOnlyList<Hazmat> _players = [];

    public SplitScreenCameraRig(PackedScene scene, Control uiParent)
    {
        _followCameraScene = scene;
        _uiParent = uiParent;
    }

    public void Setup(IReadOnlyList<Hazmat> players, Node3D _)
    {
        if (_followCameraScene == null)
        {
            throw new InvalidOperationException("No follow camera scene was given. Cannot setup follow camera");
        }
        if (_uiParent == null)
        {
            throw new InvalidOperationException("No UI parent was given. Cannot setup split screen cameras.");
        }

        _players = players;

        for (int i = 0; i < 2; i++)
        {
            // left/right
            SubViewportContainer container = new()
            {
                Stretch = true, // auto-fill
                AnchorLeft = i == 0 ? 0f : 0.5f,
                AnchorRight = i == 0 ? 0.5f : 1f,
                AnchorTop = 0f,
                AnchorBottom = 1f
            };
            // top/bottom
            // SubViewportContainer container = new()
            // {
            //     Stretch = true, // auto-fill
            //     AnchorLeft = 0f,
            //     AnchorRight = 1f,
            //     AnchorTop = i == 0 ? 0.5f : 1f,
            //     AnchorBottom = i == 0 ? 0f : 0.5f
            // };

            SubViewport subViewport = new()
            {
                World3D = _uiParent.GetTree().Root.World3D
            };

            container.AddChild(subViewport);
            _uiParent.AddChild(container);

            FollowCamera rig = _followCameraScene.Instantiate<FollowCamera>();

            try
            {
                Hazmat h = _players.First(p => p.InputDevice?.DeviceId == MatchStatus.Instance.ConfirmedPlayers[i].DeviceId);
                rig.Target = h;

                subViewport.AddChild(rig);

                if (!h.HomeTeam)
                    rig.RotateY(Mathf.Pi);
            }
            catch
            {
                throw;
            }
            
            Camera3D camera = rig.GetNode<Camera3D>("Camera Pos/Camera");
            camera.Current = true;

            _containers.Add(container);
            _rigs.Add(rig);
            _cameras.Add(camera);
        }
    }

    public Camera3D GetCameraForPlayer(int playerIndex) => _cameras[_players.First(p => p.PlayerId == playerIndex).InputDevice.DeviceId];
    public void Tick(double delta) { }

    public void Teardown()
    {
        foreach (var container in _containers)
            container.QueueFree();

        _containers.Clear();
        _rigs.Clear();
        _cameras.Clear();
    }
}