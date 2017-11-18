using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;

namespace DruggoRTS
{
    public class RWorld : Node
    {
        AStar astar = new AStar();

        public int Width  { get; private set; }
        public int Height { get; private set; }
        
        public override void _Ready()
        {
            Width  = 16;
            Height = 16;

            //Create the points
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    astar.AddPoint(Flatten(x, y), new Vector3(x, y, 0));
                }
            }

            //Connect them!
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    var topLeft = new Point2(Mathf.max(x - 1, 0), Mathf.max(y - 1, 0));
                    var bottomRight = new Point2(Mathf.max(x + 1, Width - 1), Mathf.max(y + 1, Height - 1));

                    for (var i = topLeft.X; i < bottomRight.X; i++)
                    {
                        for (var j = topLeft.Y; j < bottomRight.Y; j++)
                        {
                            astar.ConnectPoints(Flatten(x, y), Flatten(i, j));
                        }
                    }
                }
            }
        }

        int Flatten(int x, int y)
        {
            return x * Height + y;
        }

    }
}
