﻿using System;
using System.IO.Ports;

namespace Watch.Toolkit.Hardware.Arduino
{
    internal class ArduinoDriver:HardwarePlatform
    {
        public string Port { get; private set; }

        private const int BaudRate = 115200;

        private SafeSerialPort _serialPort;
        private string _output;

        private readonly string _port;

        public ArduinoDriver(string port)
        {
            _port = port;
        }
        public override void Start()
        {
            if (IsRunning) return;
            ConnectToDevice(_port);
            IsRunning = true;
        }
        public override void Stop()
        {
            _serialPort.Close();
        }

        private void ConnectToDevice(string port)
        {
            Port = port;
            ConnectToArduino(port);
        }

        ~ArduinoDriver()
        {
            if (_serialPort != null)
            {
                if (_serialPort.IsOpen)
                {
                    _serialPort.Close();
                    _serialPort.Dispose();
                }
            }
        }

        private void ConnectToArduino(string portname)
        {
            try
            {
                _serialPort = null;
                _serialPort = new SafeSerialPort(portname, BaudRate);
                _serialPort.DataReceived += serialPort_DataReceived;
                _serialPort.PinChanged += _serialPort_PinChanged;
                _serialPort.ErrorReceived += _serialPort_ErrorReceived;
                _serialPort.Open();

                Console.WriteLine("Found device at: " + portname);
            }
            catch (Exception ex)
            {
                Console.WriteLine("NOT connected to: " + portname);
                Console.WriteLine(ex.ToString());
            }
        }

        void _serialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }

        void _serialPort_PinChanged(object sender, SerialPinChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            _output += _serialPort.ReadLine();
            _output = _output.Replace("\r\n", "").Replace("\r", "").Replace("\n", "");
            if (_output.EndsWith("#"))
            {
                _output = _output.Replace("#", "");
                OnMessageReceived(this, new MessagesReceivedEventArgs(-1, _output));
            }
            _output = "";
        }
    }
}
