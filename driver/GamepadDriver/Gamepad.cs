using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace GamepadDriver
{
    public class Gamepad
    {
        private Dictionary<Keys, char> _layout;
        private Keys _currentKeys;

        public Gamepad(Dictionary<Keys, char> layout)
        {
            _currentKeys = Keys.None;
            _layout = layout;
        }

        public Keys DetermineKeys(byte data)
        {
            if (data == 0)
            {
                return Keys.None;
            }

            return data == 255 ? Keys.None : (Keys)((byte)(~data));
        }

        public void Press(Keys newKeys)
        {
            // если непонятно, какие кнопки нажаты или нажатые кнопки не поменялись
            if (newKeys == _currentKeys)
            {
                return;
            }
            
            // что-то поменялось
            foreach (var key in Enum.GetValues(typeof(Keys)).Cast<Keys>().Where(k => k != Keys.None))
            {
                if (_currentKeys != Keys.None)
                {
                    // было что-то нажато
                    // "отпускаем" старые нажатые кнопки
                    if (newKeys.HasFlag(key))
                    {
                        keybd_event(VkKeyScan(_layout[key]), 0, 2, 0);
                    }
                }

                if (newKeys != Keys.None)
                {
                    // стало что-то нажато
                    // "нажимаем" новые
                    if (newKeys.HasFlag(key))
                    {
                        keybd_event(VkKeyScan(_layout[key]), 0, 0, 0);
                    }
                }
            }

            // запоминаем новые нажатые
            _currentKeys = newKeys;
        }

        [DllImport("user32.dll")]
        public static extern void keybd_event(uint bVk, uint bScan, long dwFlags, long dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern uint VkKeyScan(char ch);
    }
}
