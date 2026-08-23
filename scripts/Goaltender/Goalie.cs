using System;
using Godot;

namespace BlueLine.Goaltender
{
    public partial class Goalie : CharacterBody3D
    {
        #region Members

        private GoalieStateMachine _stateMachine;

        private float _groundPosY;

        #endregion Members

        #region Properties

        /// <summary>
        /// The puck that the goalie tries to save.
        /// </summary>
        public Puck PuckToTrack { get; set; }

        /// <summary>
        /// The goal that the goalie stands in front of.
        /// </summary>
        [Export]
        public Node3D GoalToDefend { get; set; }

        /// <summary>
        /// How fast the goalie rotates to face the puck.
        /// </summary>
        [Export]
        public float RotationSpeed { get; set; } = 5.0f;

        // /// <summary>
        // /// Maximum distance that the goalie will move.
        // /// </summary>
        // [Export]
        // public float MaxGoalieDepth { get; set; } = 3.0f;

        // /// <summary>
        // /// How far to the sides the goalie will move.
        // /// </summary>
        // [Export]
        // public float MaxGoalieLateral { get; set; } = 2.0f;

        #endregion Properties

        #region Override

        public override void _Ready()
        {
            _stateMachine = new GoalieStateMachine();

            _stateMachine.ChangeState(
                new GoalieTrackingState(this)
            );

            _groundPosY = GlobalPosition.Y;
        }

        public override void _PhysicsProcess(double delta)
        {
            _stateMachine.PhysicsUpdate(delta);
        }

        #endregion Override

        #region Public Methods

        public void TrackThePuck(double delta)
        {
            if (PuckToTrack == null)
            {
                throw new InvalidOperationException("No puck was given to track. Cannot keep goalie square to puck.");
            }
            if (GoalToDefend == null)
            {
                throw new InvalidOperationException("No goal to defend was given. The goalie will not be able to adjust their position.");
            }

            //TODO: calculate the goaliedepth based on how far away the puck is.
            float goalieDepth = 8.0f;

            Vector3 direction = (PuckToTrack.GlobalPosition - GoalToDefend.GlobalPosition).Normalized();
            Vector3 point = GoalToDefend.GlobalPosition + direction * goalieDepth;

            //TODO: the goalie should be a little slow to shuffle side to side


            // make sure the goalie stays on the ice
            point.X = GlobalPosition.X > 0 ? Mathf.Min(GoalToDefend.GlobalPosition.X, point.X) : Mathf.Max(GoalToDefend.GlobalPosition.X, point.X);
            point.Y = _groundPosY;

            GlobalPosition = point;
        }

        /// <summary>
        /// Keep the goalie looking at the puck.
        /// </summary>
        /// <param name="delta"></param>
        /// <exception cref="InvalidOperationException">A puck needs to be setup before calling this so the goalie can track it</exception>
        public void StaySquareToPuck(double delta)
        {
            if (PuckToTrack == null)
            {
                throw new InvalidOperationException("No puck was given to track. Cannot keep goalie square to puck.");
            }

            //TODO: This will probably need to be adjusted based on home or away.

            Vector3 direction = PuckToTrack.GlobalPosition - GlobalPosition;

            // We only want horizontal rotation.
            direction.Y = _groundPosY;

            if (direction.LengthSquared() < 0.001f)
                return;

            direction = direction.Normalized();

            float targetAngle = Mathf.Atan2(
                direction.X,
                direction.Z
            );

            Rotation = new Vector3(
                0,
                Mathf.LerpAngle(
                    Rotation.Y,
                    targetAngle,
                    RotationSpeed * (float)delta
                ),
                0
            );
        }

        public void BeginSave()
        {
            //TODO: animate this
        }

        public bool UpdateSave()
        {
            //TODO: animate this
            return false;
        }

        public void ChangeState(GoalieState newState)
        {
            _stateMachine.ChangeState(newState);
        }

        public bool Recover()
        {
            //TODO: animate this
            return false;
        }

        #endregion Public Methods
    }
}