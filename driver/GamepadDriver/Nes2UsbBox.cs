using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace GamepadDriver
{
    public class Nes2UsbBox : IDisposable
    {
        private SerialPort _port;
        private bool _portOpened;
        private int _portNumber;
        
        private const int _defaultPortNumber = 2;
        private const int _defaultBaudRate = 57600;
        private const int _maxComPort = 100;
        
        public Nes2UsbBox()
        {
            Init();
        }

        public string PortName
        {
            get
            {
                return "COM" + _portNumber;
            }
        }

        public bool Connect()
        {
            while (!_portOpened && _portNumber <= _maxComPort)
            {
                _port.PortName = PortName;
                try
                {
                    _port.Open();
                }
                catch
                {
                    _portNumber++;
                    continue;
                }

                _portOpened = true;
            }

            return _portOpened;
        }

        public byte ReadGamepadState()
        {
            bool isRead = false;
            byte state = 0;
            while (!isRead)
            {
                try
                {
                    state = (byte)_port.ReadByte();
                    isRead = true;
                }
                catch (IOException)
                {
                    while (true)
                    {
                        Init();
                        var connected = Connect();
                        if (connected)
                        {
                            break;
                        }

                        Thread.Sleep(1000);
                    }
                }
            }

            return state;
        }

        public void Dispose()
        {
            _port.Close();
        }

        private void Init()
        {
            _portOpened = false;
            _portNumber = _defaultPortNumber;
            _port = new SerialPort();
            _port.BaudRate = _defaultBaudRate;
        }
    }
}
