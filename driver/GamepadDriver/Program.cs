using System;
using System.Collections.Generic;

namespace GamepadDriver
{
    class Program
    {
        static void Main(string[] args)
        {


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
                    gamepad.Handle(keys);
                }
            } 
        }
    }
}
