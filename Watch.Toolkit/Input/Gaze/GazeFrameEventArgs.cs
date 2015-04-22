// -*- mode: csharp; c-basic-offset: 4; indent-tabs-mode: nil -*-
using System;

using Watch.Toolkit.Input.Gaze;

namespace Watch.Toolkit.Input.Gaze
{
	public class GazeFrameEventArgs : EventArgs
	{
		public readonly GazeFrame Frame;

		public GazeFrameEventArgs (GazeFrame frame)
		{
			Frame = frame;
		}
	}
}
