using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TETCSharpClient.Data;

namespace Watch.Toolkit.Input.Gaze
{
    public class GazeFrame
    {
        public readonly double X, Y;
        public readonly double Roll;
        public readonly double Ipd;

        private GazeFrame(double _x, double _y, double _roll, double _ipd)
        {
            X = _x;
            Y = _y;
            Roll = _roll;
            Ipd = _ipd;
        }

        public static GazeFrame Empty = new GazeFrame(0, 0, 0, 0);

        public static GazeFrame fromGazeData(GazeData data)
        {
            return new GazeFrame(
                data.SmoothedCoordinates.X,
                data.SmoothedCoordinates.Y,
                ComputeRoll(data),
                ComputeIpd(data));
        }

        public static GazeFrame operator + (GazeFrame l, GazeFrame r)
        {
            return new GazeFrame(l.X + r.X, l.Y + r.Y, l.Roll + r.Roll, l.Ipd + r.Ipd);
        }

        public static GazeFrame operator / (GazeFrame f, double k)
        {
            return new GazeFrame(f.X / k, f.Y / k, f.Roll / k, f.Ipd / k);
        }

        private static double ComputeIpd(GazeData data)
        {
            var delta = data.LeftEye.SmoothedCoordinates.Subtract(data.RightEye.SmoothedCoordinates);
            return Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y);
        }

        private static double ComputeRoll(GazeData data)
        {
            var delta = data.LeftEye.SmoothedCoordinates.Subtract(data.RightEye.SmoothedCoordinates);
            var rollRad = (Math.Atan2(delta.Y, delta.X) + 360.0) % 360.0 + 180.0;
			var roll = rollRad * (180.0 / Math.PI);
			return Math.Abs (roll) < 90.0 ? roll : 0.0;
        }
    }
}
