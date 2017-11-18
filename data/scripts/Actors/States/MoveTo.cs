using Godot;

namespace DruggoRTS
{
    public class MoveTo : ActorStateHandler
    {
        public Vector2 GoalPos { get; private set; }

        public MoveTo(Actor actor, Vector2 pos) : base(actor)
        {
            GoalPos = pos;
        }

        public override void OnUpdate()
        {
            var distance = (actor.Position - GoalPos).length_squared();

            //If it's close enough to the destination, be idle.
            if (distance < actor.Speed * actor.Speed)
            {
                actor.OrderBeIdle();
            }
            else
            { 
                //Otherwise, keeping moving to the dest.
                actor.MoveTowards(GoalPos);
            }
        }
    }
}
