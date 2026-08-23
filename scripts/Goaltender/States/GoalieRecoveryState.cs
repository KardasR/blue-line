namespace BlueLine.Goaltender;

public class GoalieRecoveringState : GoalieState
{
    public GoalieRecoveringState(Goalie goalie)
        : base(goalie)
    {
    }

    public override void PhysicsUpdate(double delta)
    {
        if (Goalie.Recover())
        {
            Goalie.ChangeState(new GoalieTrackingState(Goalie));
        }
    }
}