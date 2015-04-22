// -*- mode: csharp; c-basic-offset: 4; indent-tabs-mode: nil -*-
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TETCSharpClient;

namespace Watch.Toolkit.Input.Gaze
{
	/// <summary>
	/// Gaze manager manages communication with The Eye Tribe tracker server
	/// and dispatches simplified gaze frames via events. The Gaze manager
	/// should be thread safe and there is no need to deal with the
	/// TET client or any of its types directly.
	/// </summary>
    public sealed class GazeManager : IInputManager
    {
		readonly object sync;
        readonly TETCSharpClient.GazeManager instance;
		readonly GazeLaundry cache;
		readonly GazeStateListener stateListener;

		// Event handlers lock explicity on "sync" to make events thread safe.
		EventHandler<GazeFrameEventArgs> newGazeFrameListeners;
		/// <summary>
		/// Occurs when new gaze frame is send by the tracker.
		/// Unsubscribing may not occur immediately due to threading.
		/// </summary>
		public event EventHandler<GazeFrameEventArgs> newGazeFrame
		{
			add { lock (sync) { newGazeFrameListeners += value; }}
			remove { lock (sync) { newGazeFrameListeners += value; }}
		}

		EventHandler<GazeStateEventArgs> gazeStateChangedListeners;
		/// <summary>
		/// Occurs when the gaze state has changed (i.e. tracking lost or active).
		/// Unsubscribing may not occur immediately due to threading.
		/// </summary>
		public event EventHandler<GazeStateEventArgs> gazeStateChanged
		{
			add { lock (sync) { gazeStateChangedListeners += value; }}
			remove { lock (sync) { gazeStateChangedListeners += value; }}
		}

		bool running;

		/// <summary>
		/// Initializes a new instance of <see cref="Watch.Toolkit.Input.Gaze.GazeManager"/>. There should
		/// only ever be one instance per program.
		/// </summary>
		/// <param name="cacheSize">Number of frames to cache for smoothing. Must be at least 1.</param>
		public GazeManager(uint cacheSize)
        {
			sync = new object();
            instance = TETCSharpClient.GazeManager.Instance;
			cache = new GazeLaundry(cacheSize);
			stateListener = new GazeStateListener(cache, OnNewGazeFrame, OnGazeStateChanged);
			instance.AddGazeListener(stateListener);
			running = false;
        }

		/// <summary>
		/// Initializes the TheEyeTribe tracker. Throws an exception
		/// if the tracker could not be activated.
		/// </summary>
        public void Start()
        {
			lock (sync)
			{
				if (!running)
				{
					running = true;
					instance.Activate(TETCSharpClient.GazeManager.ApiVersion.VERSION_1_0, TETCSharpClient.GazeManager.ClientMode.Push);
				}
			}
        }

		/// <summary>
		/// Stops the TheEyeTribe tracker.
		/// </summary>
        public void Stop()
        {
			lock (sync)
			{
				if (running)
				{
					running = false;
					instance.Deactivate();
				}
			}
        }

		void OnNewGazeFrame(GazeFrame frame)
		{
			EventHandler<GazeFrameEventArgs> evt;
			lock (sync)
			{
				evt = newGazeFrameListeners;
			}
			if (evt != null)
				evt(this, new GazeFrameEventArgs(frame));
		}

		void OnGazeStateChanged(bool tracking)
		{
			EventHandler<GazeStateEventArgs> evt;
			lock (sync)
			{
				evt = gazeStateChangedListeners;
			}
			if (evt != null)
				evt(this, new GazeStateEventArgs(tracking));
		}

    }
}
