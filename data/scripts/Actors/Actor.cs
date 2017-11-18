using Godot;

namespace DruggoRTS
{
    public enum ActorState
    {
        /// <summary>
        /// Idle means the actor will be doing nothing.
        /// </summary>
        Idle,

        /// <summary>
        /// MoveTo means the actor is (or at least is trying) to move to a specified point.
        /// </summary>
        MoveTo
    }

    public class Actor : Node2D
    {
        /// <summary>
        /// Movement speed of the actor (per frame).
        /// </summary>
        public float Speed { get; set; } = 2f;

        /// <summary>
        /// Current angle of movement.
        /// </summary>
        public float Angle { get; private set; }

        //These are used for state handling.
        ActorState state;
        ActorState nextState;
        ActorStateHandler stateHandler = null;
        ActorStateHandler nextStateHandler = null;

        public override void _Ready()
        {
            //Set the default state handler to IDLE. The state handlers give orders to their own actors.
            stateHandler = new Idle(this);
            state = ActorState.Idle;
        }

        public override void _Process(float delta)
        {
            //Change the state if requested last frame.
            if (nextStateHandler != null)
            {
                ChangeStates();
            }

            //Update state handler and _Draw()
            stateHandler.OnUpdate();
            Update();

            //Order actor to move to mouse's position if right mouse button is clicked.
            if (Input.IsMouseButtonPressed(GD.BUTTON_RIGHT))
            {
                OrderMoveTo(GetGlobalMousePosition());
            }
        }

        public override void _Draw()
        {
            if (state == ActorState.MoveTo)
            {
                //Blackmagicka
                var inv = GlobalTransform.inverse();
                DrawSetTransform(inv.Origin, inv.Rotation + Mathf.PI, inv.Scale);

                //Draw line from my position to my goalpos
                DrawLine(Position, (stateHandler as MoveTo).GoalPos, Color.Color8(255, 255, 255, 255));
            }
        }

        void ChangeStates()
        {
            //Call current stateHandler's Dispose, which's what happens before it becomes unused.
            stateHandler.OnDispose();

            //Set the current state and handler to the next.
            stateHandler = nextStateHandler;
            state = nextState;

            //Call current stateHandler (which is the new one) OnInitialize, which's what happens right after becoming the actual stateHandler.
            stateHandler.OnInitialize();

            //Nullify the next.
            nextStateHandler = null;
            nextState = ActorState.Idle;

            //DEBUG: draw the current state.
            (GetNode("State") as Label).Text = state.ToString();
        }

        #region Orders

        /// <summary>
        /// Sets the next state to be idle. Means that, from the next frame, the actor will be idle.
        /// </summary>
        public void OrderBeIdle()
        {
            nextStateHandler = new Idle(this);
            nextState = ActorState.Idle;
        }
        
        /// <summary>
        /// Sets the next state to be move-to. Means that, from the next frame, the actor will be moving to a destination.
        /// </summary>
        public void OrderMoveTo(Vector2 position)
        {
            nextStateHandler = new MoveTo(this, position);
            nextState = ActorState.MoveTo;
        }

        #endregion

        #region Actions

        /// <summary>
        /// Moves towards a specified vector, is meant to be used by state handlers only.
        /// </summary>
        /// <param name="pos"></param>
        public void MoveTowards(Vector2 pos)
        {
            var angle = (pos - Position).angle();
            MoveAtAngle(angle);
        }

        /// <summary>
        /// Moves at a specified angle, is meant to be used by state handlers only.
        /// </summary>
        /// <param name="radians"></param>
        void MoveAtAngle(float radians)
        {
            var moveAt = new Vector2(Mathf.cos(radians), Mathf.sin(radians));

            Translate(moveAt * Speed);
            Angle = radians;
        }

        #endregion

    }
}
