using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Watch.Toolkit.Input.Gaze
{
    class GazeLaundry
    {
        readonly GazeFrame[] cache;
        int p;

        public GazeLaundry(uint cacheSize)
        {
            if (cacheSize < 1)
                throw new ArgumentOutOfRangeException("cacheSize", "Must at least be 1.");
            cache = new GazeFrame[cacheSize];
            p = 0;
        }

        public void Push(GazeFrame data)
        {
            lock (this)
            {
                p = (p + 1) % cache.Length;
                cache[p] = data;
            }
        }

        public GazeFrame Smooth()
        {
            lock (this)
            {
                GazeFrame aggregate = GazeFrame.Empty;
                int n = 0;
                foreach (var frame in cache)
                {
                    if (frame != null)
                    {
                        aggregate += frame;
                        ++n;
                    }
                }
                return n > 0 ? aggregate / n : GazeFrame.Empty;
            }
        }

        public GazeFrame Raw()
        {
            lock (this)
            {
                return cache[p];
            }
        }
    }
}
