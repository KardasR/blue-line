using Godot;

namespace BlueLine;

[GlobalClass] // Makes the resource discoverable in the Godot Editor
public partial class PlayerAttributes : Resource
{
    #region Skating

    [Export]
    public float SkatingSpeed { get; set; } = 20.0f;

    [Export]
    public float Acceleration { get; set; } = 20.0f;

    [Export]
    public float Deceleration { get; set; } = 40.0f;

    [Export]
    public float BackwardSpeed { get; set; } = 10.0f;

    [Export]
    public float BackwardAcceleration { get; set; } = 5.0f;

    [Export]
    public float TurnSpeed { get; set; } = 4.0f;

    [Export]
    public float SprintSpeed { get; set; } = 30.0f;

    #endregion Skating

    #region Shooting

    /// <summary>
    /// How hard the player shoots.
    /// </summary>
    [Export]
    public float ShotSpeed { get; set; } = 50.0f;

    /// <summary>
    /// How hard the player passes the puck.
    /// </summary>
    [Export]
    public float PassSpeed { get; set; } = 20.0f;

    /// <summary>
    /// What to multiply a players typical shot speed by to get a slapshot speed.
    /// </summary>
    [Export]
    public float SlapshotMultiplier { get; set; } = 1.5f;

    #endregion Shooting

    #region Stick Handling

    /// <summary>
    /// How far away from the center point the player dangles the puck.
    /// </summary>
    [Export]
    public float StickHandleRange { get; set; } = 1.0f;

    /// <summary>
    /// How fast the player will move the puck when stick handling.
    /// </summary>
    [Export]
    public float StickHandleSpeed { get; set; } = 30.0f;

    /// <summary>
    /// How hard the player pokes the puck
    /// </summary>
    [Export]
    public float PokeCheckForce { get; set; } = 20.0f;

    #endregion Stick Handling

    #region Goalie

    /// <summary>
    /// How close the goalie tries to get on the intersection of the puck and goal.
    /// </summary>
    [Export]
    public float PositionTolerance { get; set; } = 0.5f;

    /// <summary>
    /// The furthest out the goalie will move.
    /// </summary>
    [Export]
    public float MaxDepth { get; set; } = 11;

    #endregion Goalie
}