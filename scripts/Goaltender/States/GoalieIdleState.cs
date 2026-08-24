namespace BlueLine.Goaltender;

public class GoalieIdleState : GoalieState
{
    public GoalieIdleState(Goalie goalie) : base(goalie)
    {
        
    }

    public override void Enter()
    {
        Goalie.StopMovement();
        Goalie.PrepareForFaceoff(FaceoffDot.CenterIce); // for now just default to center ice
    }
}