namespace BlueLine.Goaltender
{
    public class GoalieStateMachine
    {
        private GoalieState _currentState;

        public void ChangeState(GoalieState newState)
        {
            _currentState?.Exit();

            _currentState = newState;

            _currentState.Enter();
        }

        public void PhysicsUpdate(double delta)
        {
            _currentState?.PhysicsUpdate(delta);
        }
    }
}