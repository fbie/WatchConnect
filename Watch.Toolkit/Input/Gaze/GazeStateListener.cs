using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TETCSharpClient;
using TETCSharpClient.Data;

namespace Watch.Toolkit.Input.Gaze
{
    class GazeStateListener: IGazeListener
    {
        public delegate void RaiseGazeFrame(GazeFrame frame);
        public delegate void RaiseGazeState(bool tracking);

        readonly GazeLaundry cache;
        readonly RaiseGazeFrame raiseFrame;
        readonly RaiseGazeState raiseState;
        bool tracking;

        public GazeStateListener(GazeLaundry _cache, RaiseGazeFrame _raiseFrame, RaiseGazeState _raiseState)
        {
            cache = _cache;
            raiseFrame = _raiseFrame;
            raiseState = _raiseState;
            tracking = false;
        }

        public void OnGazeUpdate(GazeData gazeData)
        {
            bool valid = IsValid(gazeData);
            if (tracking != valid)
            {
                raiseState(valid);
                tracking = valid;
            }
            if (valid)
            {
                GazeFrame frame = GazeFrame.fromGazeData(gazeData);
                cache.Push(frame);
                raiseFrame(cache.Smooth());
            }
        }

        private static bool IsValid(GazeData gazeData)
        {
            return (gazeData.State | (GazeData.STATE_TRACKING_EYES & GazeData.STATE_TRACKING_GAZE)) != 0;
        }
    }
}
