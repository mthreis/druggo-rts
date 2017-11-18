using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DruggoRTS
{
    public class Randomizer
    {
        static Random random;

        static Randomizer()
        {
            random = new Random();
        }

        public static int Get(int minValue, int maxValue)
        {
            return random.Next(minValue, maxValue);
        }
    }
}
