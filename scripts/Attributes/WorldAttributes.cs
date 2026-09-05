using Godot;

namespace BlueLine;

[GlobalClass] // Makes the resource discoverable in the Godot Editor
public partial class WorldAttributes : Resource
{
    /// <summary> 
    /// The downward acceleration when in the air, in meters per second squared.
    /// </summary> 
    [Export] 
    public uint FallAcceleration { get; set; } = 75;

    /// <summary>
    /// How much the player slows due to ice.
    /// </summary>
    [Export]
    public float IceFriction { get; set; } = 3.0f;

    /// <summary>
    /// How long a user has to hold the shoot button to take a slapshot.
    /// </summary>
    [Export]
    public uint SlapshotThreshold { get; set; } = 15;

    /// <summary>
    /// How high or low the user has to move the right stick to trigger a wind up or shot.
    /// </summary>
    [Export]
    public float ShotDeadzone { get; set; } = 0.5f;

    /// <summary>
    /// How hard the puck is pushed after a body check is landed.
    /// </summary>
    [Export]
    public float BodyCheckPuckDropForce { get; set; } = 10.0f;
}