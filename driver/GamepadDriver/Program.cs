using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;

namespace GamepadDriver
{
    class Program
    {
        static void Main(string[] args)
        {
            var gamepad1 = new Gamepad(new Dictionary<Keys, char>
            {
                { Keys.Right, 'D' },
                { Keys.Left, 'A' },
                { Keys.Down, 'S' },
                { Keys.Up, 'W' },
                { Keys.Start, ' ' },
                { Keys.Select, 'N' },
                { Keys.B, 'J' },
                { Keys.A, 'K' }
            });

            var gamepad2 = new Gamepad(new Dictionary<Keys, char>()
            {
                { Keys.Right, 'B' },
                { Keys.Left, 'C' },
                { Keys.Down, 'V' },
                { Keys.Up, 'F' },
                { Keys.Start, 'H' },
                { Keys.Select, 'M' },
                { Keys.B, 'Y' },
                { Keys.A, 'U' }
            });

            var gamepads = new[] { gamepad1, gamepad2 };

            var box = new Nes2UsbBox();
            if (!box.Connect())
            {
                Console.WriteLine("Не удалось подключиться к устройству");
                return;
            }

            Console.WriteLine("Подключение установлено на {0}", box.PortName);
            
            while (true)
            {
                foreach (var gamepad in gamepads)
                {
                    var state = box.ReadGamepadState();
                    var keys = gamepad.DetermineKeys(state);
                    gamepad.Press(keys);
                }
            } 
        }
    }
}
