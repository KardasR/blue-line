using BlueLine.Skater;
using Godot;
using System.Collections.Generic;

namespace BlueLine.VideoFeed;

public interface ICameraRig
{
    void Setup(IReadOnlyList<Hazmat> players, Node3D puck);

    void Teardown();

    Camera3D GetCameraForPlayer(int playerIndex);
    
    void Tick(double delta);
}