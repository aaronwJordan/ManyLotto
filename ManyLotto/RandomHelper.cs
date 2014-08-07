using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ManyLotto
{
    public static class RandomHelper
    {
		private static int seedCounter = new Random().Next();

        [ThreadStatic] private static Random rng;

        public static Random Instance
        {
            get
            {
                if (rng == null)
                {
                    int seed = Interlocked.Increment(ref seedCounter);
                    rng = new Random(seed);
                }
                return rng;
            }
        }
    }
}
