using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace GamepadDriver
{
    public abstract class GamepadBase
    {
        protected Keys _currentKeys;

        public GamepadBase()
        {
            _currentKeys = Keys.None;
        }

        public Keys DetermineKeys(byte data)
        {
            if (data == 0)
            {
                return Keys.None;
            }

            return data == 255 ? Keys.None : (Keys)((byte)(~data));
        }

        public void Handle(Keys newKeys)
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
                        Release(key);
                    }
                }

                if (newKeys == (Keys.Start | Keys.Select))
                {
                    if (StartAndSelect != null)
                    {
                        StartAndSelect();
                    }
                }
                else if (newKeys != Keys.None && newKeys != _currentKeys)
                {
                    // стало что-то нажато
                    // "нажимаем" новые
                    if (newKeys.HasFlag(key))
                    {
                        Press(key);
                    }
                }
            }

            // запоминаем новые нажатые
            _currentKeys = newKeys;
        }

        public event Action StartAndSelect;

        public abstract void Press(Keys key);

        public abstract void Release(Keys key);
    }

    public class NesGamepad : GamepadBase
    {
        private enum KeyboardEvent
        {
            Press = 0,
            Release = 2
        }

        private Dictionary<Keys, char> _layout;

        public NesGamepad(Dictionary<Keys, char> layout) : base()
        {
            _layout = layout;
        }

        [DllImport("user32.dll")]
        public static extern void keybd_event(uint bVk, uint bScan, long dwFlags, long dwExtraInfo);

        [DllImport("user32.dll")]
        public static extern uint VkKeyScan(char ch);

        public override void Press(Keys key)
        {
            Keyboard(key, KeyboardEvent.Press);
        }

        public override void Release(Keys key)
        {
            Keyboard(key, KeyboardEvent.Release);
        }

        private void Keyboard(Keys key, KeyboardEvent e)
        {
            keybd_event(VkKeyScan(_layout[key]), 0, (int)e, 0);
        }
    }
}
