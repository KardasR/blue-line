using System;
using Godot;
using GodotPlugins.Game;

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
        /// 
        /// </summary>
        [Export]
        public Goal Net { get; set; }

        #region Tracking

        /// <summary>
        /// 
        /// </summary>
        [Export]
        public float MaxDepth { get; set; } = 11;

        /// <summary>
        /// 
        /// </summary>
        [Export]
        public Node3D LeftMostPos { get; set; }
        
        /// <summary>
        /// 
        /// </summary>
        [Export]
        public Node3D RightMostPos { get; set; }

        #endregion Tracking

        #region Skating

        /// <summary>
        /// How fast the goalie rotates to face the puck.
        /// </summary>
        [Export]
        public float RotationSpeed { get; set; } = 5.0f;

        /// <summary>
        /// 
        /// </summary>
        [Export]
        public float Speed { get; set; } = 20.0f;

        /// <summary>
        /// 
        /// </summary>
        [Export]
        public float Acceleration { get; set; } = 20.0f;

        /// <summary>
        /// 
        /// </summary>
        [Export]
        public float StoppingAcceleration { get; set; } = 40.0f;

        /// <summary>
        /// 
        /// </summary>
        [Export]
        public float IceFriction { get; set; } = 3.0f;

        /// <summary>
        /// 
        /// </summary>
        [Export]
        public float PositionTolerance { get; set; } = 0.1f;

        #endregion Skating

        #endregion Properties

        #region Override

        public override void _Ready()
        {
            if (Net == null)
            {
                throw new InvalidOperationException("No net was given. Cannot detect any goals");
            }
            if (Owner is not MainNode)
            {
                throw new InvalidCastException($"Owner is not a MainNode. It's a : {Owner.GetType()}");
            }

            Net.GoalScored += OnGoalScored;
            (Owner as MainNode).FaceoffStarted += OnFaceoffStarted;            

            _groundPosY = GlobalPosition.Y;
            _stateMachine = new GoalieStateMachine();

            ChangeState(new GoalieTrackingState(this));
        }

        public override void _PhysicsProcess(double delta)
        {
            _stateMachine.PhysicsUpdate(delta);
        }

        #endregion Override

        #region Events

        private void OnGoalScored(object sender, EventArgs e)
        {
            ChangeState(new GoalieIdleState(this));
        }

        private void OnFaceoffStarted(object sender, EventArgs e)
        {
            ChangeState(new GoalieTrackingState(this));
        }

        #endregion Events

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
            if (LeftMostPos == null)
            {
                throw new InvalidOperationException("No left most position was given. Cannot move goalie");
            }
            if (RightMostPos == null)
            {
                throw new InvalidOperationException("No right most position was given. Cannot move goalie");
            }
            //TODO: calculate the goaliedepth based on how far away the puck is.
            float goalieDepth = 8.0f;

            Vector3 direction = (PuckToTrack.GlobalPosition - GoalToDefend.GlobalPosition).Normalized();
            Vector3 point = GoalToDefend.GlobalPosition + direction * goalieDepth;

            //TODO: This needs to properly work for both goalies
            // constrain depth
            point.X = Mathf.Clamp(point.X, GoalToDefend.GlobalPosition.X, MaxDepth);

            // constrain horizontal
            point.Z = Mathf.Clamp(point.Z, LeftMostPos.GlobalPosition.Z, RightMostPos.GlobalPosition.Z);

            //point.X = GlobalPosition.X > 0 ? Mathf.Min(GoalToDefend.GlobalPosition.X, point.X) : Mathf.Max(GoalToDefend.GlobalPosition.X, point.X);
            
            //GlobalPosition = point;

            Vector3 horizontalVelocity = new Vector3(
                Velocity.X,
                0,
                Velocity.Z
            );

            Vector3 movementDirection = point - GlobalPosition;
            movementDirection.Y = _groundPosY;

            if (movementDirection.Length() > PositionTolerance)
            {
                // make sure the goalie stays on the ice
                movementDirection = movementDirection.Normalized();

                Vector3 desiredVelocity = movementDirection * Speed;

                float accel = Acceleration;

                // Are we trying to move against our current momentum?
                if (horizontalVelocity.Dot(movementDirection) < 0)
                {
                    accel = StoppingAcceleration;
                }

                horizontalVelocity = horizontalVelocity.MoveToward(
                    desiredVelocity,
                    accel * (float)delta
                );
            }
            else
            {
                horizontalVelocity = horizontalVelocity.MoveToward(
                    Vector3.Zero,
                    IceFriction * StoppingAcceleration * (float)delta
                );
            }

            Velocity = new Vector3(
                horizontalVelocity.X,
                Velocity.Y,
                horizontalVelocity.Z
            );

            if (!IsOnFloor())
            {
                Velocity = new Vector3(
                    Velocity.X,
                    Velocity.Y - 75f * (float)delta,
                    Velocity.Z
                );
            }

            MoveAndSlide();
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

            //TODO: clamp between a min and max angle so the goalie doesn't fully turn around.
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

        public void StopMovement()
        {
            Velocity = Vector3.Zero;
        }

        public void PrepareForFaceoff(FaceoffDot faceoffDot)
        {
            // TODO: lineup facing where the faceoff will be
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