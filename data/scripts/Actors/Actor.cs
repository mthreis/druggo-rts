using Godot;
using System.Collections.Generic;

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

    public enum MoveWith { Left, Middle, Right, Random }

    public class Actor : Node2D
    {
        [Export]
        public MoveWith moveWith;

        public Circle CollisionBox { get; set; }

        /// <summary>
        /// Movement speed of the actor (per frame).
        /// </summary>
        public float Speed { get; set; } = 2f;

        /// <summary>
        /// Current angle of movement.
        /// </summary>
        public float Angle { get; private set; }

        /// <summary>
        /// Actor's size, used for avoidance.
        /// </summary>
        public const float Size = 32;

        public const float CollisionCheckAhead = 12;

        readonly float[] avoidanceAngles =
        {
            //first the actor tries to move straight (no additions to its move angle)
            0,

            //then, if there's an obstacle at that angle, tries to find free spots at 30 degrees of each side
             Mathf.PI / 6f,
            -Mathf.PI / 6f,

            //then, 60
             Mathf.PI / 3f,
            -Mathf.PI / 3f,

            //then 90
             Mathf.PI / 2f,
            -Mathf.PI / 2f
        };

        //not used yet
        readonly int[] AvoidanceL = { 1, 3, 5 };
        readonly int[] AvoidanceR = { 2, 4, 6 };

        public Color DrawLineColor { get; set; }

        //These are used for state handling.
        ActorState state;
        ActorState nextState;
        ActorStateHandler stateHandler = null;
        ActorStateHandler nextStateHandler = null;

        public override void _Ready()
        {
            CollisionBox = new Circle(Position, Size);

            DrawLineColor = new Color(1, 1, 1);

            //Set the default state handler to IDLE. The state handlers give orders to their own actors.
            stateHandler = new Idle(this);
            state = ActorState.Idle;
        }

        public override void _Process(float dt)
        {
            //Change the state if requested last frame.
            if (nextStateHandler != null)
            {
                ChangeStates();

                //DEBUG: draw the current state.
                this.GetNodeOf<Label>("State").Text = state.ToString();
            }

            //Update state handler and _Draw()
            Update();
            stateHandler.OnUpdate(dt);

            //Order actor to move to mouse's position if right mouse button is clicked.
            if (moveWith == MoveWith.Right)
            {
                if (Input.IsMouseButtonPressed(GD.BUTTON_RIGHT))
                {
                    OrderMoveTo(GetGlobalMousePosition());
                }
            }
            else if (moveWith == MoveWith.Left)
            {
                if (Input.IsMouseButtonPressed(GD.BUTTON_LEFT))
                {
                    OrderMoveTo(GetGlobalMousePosition());
                }
            }
            else if (moveWith == MoveWith.Middle)
            {
                if (Input.IsMouseButtonPressed(GD.BUTTON_MIDDLE))
                {
                    OrderMoveTo(GetGlobalMousePosition());
                }
            }
            else if (moveWith == MoveWith.Random)
            {
                if (state == ActorState.Idle)
                {
                    OrderMoveTo(new Vector2(Randomizer.Get(100, 924), Randomizer.Get(100, 500)));
                }
            }
        }

        public override void _Draw()
        {
            //Blackmagicka
            var inv = GlobalTransform.inverse();
            DrawSetTransform(inv.Origin, inv.Rotation + Mathf.PI, inv.Scale);

            DrawCircle(CollisionBox.Position, CollisionBox.Radius, Color.Color8(255, 255, 255, 255));

            if (state == ActorState.MoveTo)
            {
                //Draw line from my position to my goalpos
                DrawLine(Position, (stateHandler as MoveTo).GoalPos, DrawLineColor);
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
        }

        #region Collision Checking

        /// <summary>
        /// Check collisions with dynamic objects (such as actors).
        /// </summary>
        /// <returns></returns>
        public bool IsCollidingWithDynamic(Vector2 pos, float offset)
        {
            var p = GetParent();
            var checkAt = Position + GetPointInAngle(offset, GetAngleTo(pos));

            foreach (Node2D actor in p.GetChildren())
            {
                if (actor != this)
                {
                    var distance = (checkAt - actor.Position).length_squared();

                    if (distance < Size * Size)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool IsCollidingWithActor(Actor actor)
        {
            var distance = (CollisionBox.Position - actor.CollisionBox.Position).length_squared();
            var radius = CollisionBox.Radius + actor.CollisionBox.Radius;

            return (distance <= radius * radius);
        }

        public bool IsBoxCollidingWithActors(Circle box, List<Actor> actors)
        {
            foreach(var actor in actors)
            {
                if (box.IsCollidingWith(actor.CollisionBox))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsCollidingWithActor(Circle collisionBox, Actor actor)
        {
            var distance = (collisionBox.Position - actor.CollisionBox.Position).length_squared();
            var radius = collisionBox.Radius + actor.CollisionBox.Radius;

            return (distance <= radius * radius);
        }


        public bool IsCollidingWithActorsInAngle(float addToAngle)
        {
            var p = GetParent();
            //var checkAt = CollisionBox.Position + GetPointInAngle(offset, angle);
            //var addAngle = 0f;


            var cBox = new Circle(Position + GetPointInAngle(Speed * CollisionCheckAhead, Angle + addToAngle), CollisionBox.Radius);

            foreach (Node2D actor in p.GetChildren())
            {
                if (actor is Actor && actor != this)
                {
                    if (IsCollidingWithActor(cBox, actor as Actor))
                    {
                        return true;
                    }
                }
            }

            CollisionBox = cBox;
            return false;
        } 

        public bool IsCollidingInAngle(float angle, float offset)
        {
            var p = GetParent();
            var checkAt = Position + GetPointInAngle(offset, angle);

            foreach (Node2D actor in p.GetChildren())
            {
                if (actor != this)
                {
                    var distance = (checkAt - actor.Position).length_squared();

                    if (distance < Size * Size)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion

        #region Orders

        /// <summary>
        /// Sets the next state to be idle. Means that, from the next frame, the actor will be idle.
        /// </summary>
        public void OrderBeIdle()
        {
            CollisionBox = new Circle(Position, CollisionBox.Radius);

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
        /// Moves towards a specified vector, is meant to be used by state handlers only. Returns if the actor can move or not.
        /// </summary>
        /// <param name="pos"></param>
        public bool MoveTowards(Vector2 pos)
        {
            //Get the angle from self to goal position
            var angle = (pos - Position).angle();
            int i = 0;

            var actors = GetActors();

            //Create a temp box to check for collisions in the specified angles
            var cBox = new Circle(Position + GetPointInAngle(CollisionCheckAhead * Speed, angle + avoidanceAngles[i]), CollisionBox.Radius);

            while (IsBoxCollidingWithActors(cBox, actors))
            {
                //GD.print("There's collision at " + Mathf.rad2deg(angle + avoidanceAngles[i]));
                i++;

                //Update the box position to check collisions in another angle
                cBox = new Circle(Position + GetPointInAngle(CollisionCheckAhead * Speed, angle + avoidanceAngles[i]), CollisionBox.Radius);

                //If there's collision in any of the possible angles, return false and let the state handler deal with that
                if (i >= avoidanceAngles.Length - 1)
                {
                    CollisionBox = new Circle(Position, CollisionBox.Radius);
                    return false;
                }
            }

            //If it has gone that far, the actor managed to find a free spot!

            //Move at the free angle, update the bounding box and return true
            MoveAtAngle(angle + avoidanceAngles[i]);
            CollisionBox = cBox;
            return true;

            /*
            var i = 0;

            while(IsCollidingWithActorsInAngle(avoidanceAngles[i]))
            {
                //GD.print("There's collision at " + Mathf.rad2deg(angle + avoidanceAngles[i]));
                i++;

                if (i >= avoidanceAngles.Length)
                {
                    CollisionBox = new Circle(Position, CollisionBox.Radius);
                    return false;
                }
            }

            //GD.print("I CAN MOVE AT " + Mathf.rad2deg(angle + avoidanceAngles[i]));
            MoveAtAngle(angle + avoidanceAngles[i]);
            CollisionBox = new Circle(Position + GetPointInAngle(Speed * 16f, Angle), CollisionBox.Radius);
            return true;
            */
        }

        public List<Actor> GetActors(bool includeSelf = false)
        {
            var actors = new List<Actor>();

            foreach (Node2D actor in GetParent().GetChildren())
            {
                if (actor is Actor && (actor != this || includeSelf))
                {
                    actors.Add(actor as Actor);
                }
            }

            return actors;
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

        Vector2 GetPointInAngle(float length, float radians)
        {
            return new Vector2(Mathf.cos(radians) * length, Mathf.sin(radians) * length);
        }

        #endregion

    }
}
