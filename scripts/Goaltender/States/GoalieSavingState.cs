namespace BlueLine.Goaltender;

public class GoalieSavingState : GoalieState
{
    // TODO: this will be used when I start adding animations

    public GoalieSavingState(Goalie goalie)
        : base(goalie)
    {
    }

    public override void Enter()
    {
        Goalie.BeginSave();
    }

    public override void PhysicsUpdate(double delta)
    {
        Goalie.StaySquareToPuck(delta);
        Goalie.TrackThePuck(delta);
    }
}