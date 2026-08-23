namespace BlueLine.Goaltender;

public class GoalieSavingState : GoalieState
{
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
        if (Goalie.UpdateSave())
        {
            Goalie.ChangeState(new GoalieRecoveringState(Goalie));
        }
    }
}