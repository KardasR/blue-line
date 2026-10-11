using Godot;

namespace BlueLine.Skaters;

public partial class AiInput : BaseInput
{
    public void UpdateMovement(Vector2 movement)
    {
        Movement = movement;
    }

    public void SkateForward()
    {
        Movement = new() { X=0, Y=1 };
    }
}