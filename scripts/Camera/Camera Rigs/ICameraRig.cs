using BlueLine.Skaters;

using Godot;

using System.Collections.Generic;

namespace BlueLine.VideoFeed;

public interface ICameraRig
{
    void Setup(IReadOnlyList<Skater> players);

    void Teardown();

    Camera3D GetCameraForPlayer(int playerIndex);
    
    void Tick(double delta);
}