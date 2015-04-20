using System;

namespace Watch.Toolkit.Input.Gaze
{
	public class GazeStateEventArgs: EventArgs
	{
		public readonly bool IsTracking;

		public GazeStateEventArgs (bool tracking)
		{
			IsTracking = tracking;
		}
	}
}

