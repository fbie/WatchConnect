﻿using System;
using System.Globalization;
using System.Timers;
using Watch.Toolkit.Hardware;

namespace Watch.Toolkit.Sensors
{
    public class ImuParser
    {
        public event EventHandler<ImuDataReceivedEventArgs> AccelerometerDataReceived = delegate { };
        
        private Imu _imu;
        private Timer _timer= new Timer(2000);
        private double _offset = 0d;

        public ImuParser(HardwarePlatform hardware)
        {
            Hardware = hardware;
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }

        void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _offset = Math.Round(_offset + 0.01, 2);
        }

        public HardwarePlatform Hardware { get; private set; }
        public void Start()
        {
            Hardware.DataPacketReceived += _arduino_DataPacketReceived;
            Hardware.AddPacketListener("IMU",
                (message) =>
                {
                    if (message.StartsWith("A"))
                        return message.Split(',').Length == 13;
                    return false;
                },
                (message) => new DataPacket(message.Split(',')));
            Hardware.Start();

            _imu = new Imu();
        }

        private void _arduino_DataPacketReceived(object sender, Hardware.DataPacketReceivedEventArgs e)
        {
            switch (e.DataPacket.Header)
            {
                case "A":
                    _imu.Update(new Vector(
                        Convert.ToDouble(e.DataPacket.Body[0], CultureInfo.InvariantCulture),
                        Convert.ToDouble(e.DataPacket.Body[1], CultureInfo.InvariantCulture),
                        Convert.ToDouble(e.DataPacket.Body[2], CultureInfo.InvariantCulture)),
                        new Vector(
                            Convert.ToDouble(e.DataPacket.Body[3], CultureInfo.InvariantCulture),
                            Convert.ToDouble(e.DataPacket.Body[4], CultureInfo.InvariantCulture),
                            Convert.ToDouble(e.DataPacket.Body[5], CultureInfo.InvariantCulture)),
                        new Vector(
                            Convert.ToDouble(e.DataPacket.Body[6], CultureInfo.InvariantCulture) + _offset,
                            Convert.ToDouble(e.DataPacket.Body[7], CultureInfo.InvariantCulture),
                            Convert.ToDouble(e.DataPacket.Body[8], CultureInfo.InvariantCulture)),
                        new Vector(
                            Convert.ToDouble(e.DataPacket.Body[9], CultureInfo.InvariantCulture),
                            Convert.ToDouble(e.DataPacket.Body[10], CultureInfo.InvariantCulture),
                            Convert.ToDouble(e.DataPacket.Body[11], CultureInfo.InvariantCulture)));

                    AccelerometerDataReceived(this, new ImuDataReceivedEventArgs(_imu));
                    break;
            }
        }

        public void Stop()
        {
            Hardware.Stop();
        }
    }
}
