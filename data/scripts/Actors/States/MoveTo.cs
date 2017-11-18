using Godot;

namespace DruggoRTS
{
    public class MoveTo : ActorStateHandler
    {
        public Vector2 GoalPos { get; private set; }

        /// <summary>
        /// Time the actor wait before trying to move again.
        /// </summary>
        float waitTime = 1f;
        float wait = 0f;

        /// <summary>
        /// Whether the actor should wait to move again.
        /// </summary>
        bool isWaiting = false;

        public MoveTo(Actor actor, Vector2 pos) : base(actor)
        {
            GoalPos = pos;
        }

        public override void OnUpdate(float dt)
        {
            if (isWaiting)
            {
                if (wait <= 0)
                {
                    actor.DrawLineColor = new Color(1, 1, 1);
                    isWaiting = false;
                }
                else
                {
                    wait -= dt;
                    return;
                }
            }

            var distance = (actor.Position - GoalPos).length_squared();

            //If it's close enough to the destination, be idle.
            if (distance < actor.Speed * actor.Speed)
            {
                actor.OrderBeIdle();
            }
            else
            { 
                //Otherwise, keeping moving to the dest.
                var move = actor.MoveTowards(GoalPos);

                if (!move)
                {
                    actor.DrawLineColor = new Color(1, 0, 0);
                    isWaiting = true;
                    wait = waitTime;
                }
            }
        }

        public override void OnDispose()
        {
            actor.DrawLineColor = new Color(1, 1, 1);
        }
    }
}
