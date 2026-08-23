using Godot;

namespace BlueLine;

public partial class CameraRig : Node3D
{
    #region Properties
    
    /// <summary>
    /// What the camera follows.
    /// </summary>
    [Export]
    public Node3D Target { get; set; }

    /// <summary>
    /// How fast the camera follows the Target.
    /// </summary>
    [Export]
    public float FollowSpeed { get; set; } = 10.0f;

    #endregion Properties

    #region Overrides

    /// <summary>
    /// 
    /// </summary>
    /// <param name="delta"></param>
    public override void _Process(double delta)
    {
        if (Target == null)
        {
            return;
        }

        GlobalPosition = GlobalPosition.Lerp(
            Target.GlobalPosition,
            FollowSpeed * (float)delta
        );
    }

    #endregion Overrides
}
