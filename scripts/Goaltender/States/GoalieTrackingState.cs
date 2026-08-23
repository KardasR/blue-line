namespace BlueLine.Goaltender;

/// <summary>
/// This class will be the tracking state where the goalie will track the puck.
/// </summary>
public class GoalieTrackingState : GoalieState
{
    public GoalieTrackingState(Goalie goalie) : base(goalie)
    {
        
    }

    public override void PhysicsUpdate(double delta)
    {
        Goalie.TrackThePuck(delta);
        Goalie.StaySquareToPuck(delta);
    }
}