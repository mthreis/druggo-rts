using Godot;

namespace DruggoRTS
{
    public struct Circle
    {
        public Vector2 Position { get;}
        public float Radius { get; }

        public Circle(Vector2 position, float radius)
        {
            Position = position;
            Radius = radius;
        }

        public bool IsCollidingWith(Circle circle)
        {
            var distance = (Position - circle.Position).length_squared();
            var radius = Radius + circle.Radius;

            return (distance <= radius * radius);
        }
    }
}
