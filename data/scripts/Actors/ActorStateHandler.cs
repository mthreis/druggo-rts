using Godot;

namespace DruggoRTS
{
    public class ActorStateHandler
    {
        protected Actor actor;

        public ActorStateHandler(Actor actor)
        {
            this.actor = actor;
        }

        /// <summary>
        /// What happens as soon as the actor's state is changed to this one.
        /// </summary>
        public virtual void OnInitialize()
        {

        }

        /// <summary>
        /// What happens every frame.
        /// </summary>
        public virtual void OnUpdate()
        {

        }

        /// <summary>
        /// What happens as soon as the actor's state is changed from this one to another.
        /// </summary>
        public virtual void OnDispose()
        {

        }
    }
}
