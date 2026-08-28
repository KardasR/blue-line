using Godot;
using System.Collections.Generic;

namespace BlueLine.VideoFeed;

public interface ICameraRig
{
    void Setup(IReadOnlyList<Node3D> players, Node3D puck);

    void Teardown();

    Camera3D GetCameraForPlayer(int playerIndex);
    
    void Tick(double delta);
}