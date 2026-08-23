namespace BlueLine.Goaltender
{
    public abstract class GoalieState
    {
        protected Goalie Goalie { get; }

        protected GoalieState(Goalie goalie)
        {
            Goalie = goalie;
        }

        /// <summary>
        /// What happens when the goalie enters a new state.
        /// </summary>
        public virtual void Enter()
        {
            
        }

        /// <summary>
        /// What happens when the goalie exits a state.
        /// </summary>
        public virtual void Exit()
        {
            
        }

        /// <summary>
        /// What the goalie should be doing based on the current state.
        /// </summary>
        /// <param name="delta"></param>
        public virtual void PhysicsUpdate(double delta)
        {
            
        }
    }
}