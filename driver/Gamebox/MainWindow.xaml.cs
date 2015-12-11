using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using GamepadDriver;
using WindowsInput;
using WindowsInput.Native;

namespace Gamebox
{
    public partial class MainWindow : Window
    {
        private GamesViewModel _gamesViewModel;
        private Thread thread;
        private GamepadBase[] _gamepads;
        private Process emulator;

        public GamesViewModel GamesViewModel
        {
            get
            {
                return _gamesViewModel;
            }
        }

        public MainWindow()
        {
            _gamesViewModel = new GamesViewModel();
            InitializeComponent();
            DataContext = GamesViewModel;
            thread = new Thread(Listen);
            thread.Start();
        }
        
        public void Listen()
        {
            InitWpfGamepads();

            var box = new Nes2UsbBox();

            if (!box.Connect())
            {
                return;
            }

            while (true)
            {
                foreach (var gamepad in _gamepads)
                {
                    var state = box.ReadGamepadState();
                    var keys = gamepad.DetermineKeys(state);
                    gamepad.Handle(keys);
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                var pi = new ProcessStartInfo(@"C:\Users\BIA\Desktop\FCE_Ultra\fceux.exe", "\"" + _gamesViewModel.GetRomPath(GamesListBox.SelectedIndex) + "\"")
                {
                    WindowStyle = ProcessWindowStyle.Maximized,
                    UseShellExecute = false
                };
                emulator = Process.Start(pi);
                Hide();
                ShowWindow(emulator.MainWindowHandle, ShowWindowCommands.Show);
                InitNesGamepads();
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
        }

        private void InitNesGamepads()
        {
            var gamepad1 = new NesGamepad(new Dictionary<Keys, char>
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
            gamepad1.StartAndSelect += Gamepad1_StartAndSelect;

            var gamepad2 = new NesGamepad(new Dictionary<Keys, char>()
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

            _gamepads = new[] { gamepad1, gamepad2 };
        }

        private void Gamepad1_StartAndSelect()
        {
            try
            {
                emulator.Kill();
            }
            catch { }
            InitWpfGamepads();
            Dispatcher.Invoke(showWindow, this);
        }

        Action<Window> showWindow = delegate(Window window)
        {
            window.Show();
        };

        private void InitWpfGamepads()
        {
            var g1 = new WpfGamepad();
            g1.StartAndSelect += () =>
            {
            };
            _gamepads = new[] { g1, new WpfGamepad() };
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow);

        enum ShowWindowCommands
        {
            /// <summary>
            /// Hides the window and activates another window.
            /// </summary>
            Hide = 0,
            /// <summary>
            /// Activates and displays a window. If the window is minimized or 
            /// maximized, the system restores it to its original size and position.
            /// An application should specify this flag when displaying the window 
            /// for the first time.
            /// </summary>
            Normal = 1,
            /// <summary>
            /// Activates the window and displays it as a minimized window.
            /// </summary>
            ShowMinimized = 2,
            /// <summary>
            /// Maximizes the specified window.
            /// </summary>
            Maximize = 3, // is this the right value?
                          /// <summary>
                          /// Activates the window and displays it as a maximized window.
                          /// </summary>       
            ShowMaximized = 3,
            /// <summary>
            /// Displays a window in its most recent size and position. This value 
            /// is similar to <see cref="Win32.ShowWindowCommand.Normal"/>, except 
            /// the window is not activated.
            /// </summary>
            ShowNoActivate = 4,
            /// <summary>
            /// Activates the window and displays it in its current size and position. 
            /// </summary>
            Show = 5,
            /// <summary>
            /// Minimizes the specified window and activates the next top-level 
            /// window in the Z order.
            /// </summary>
            Minimize = 6,
            /// <summary>
            /// Displays the window as a minimized window. This value is similar to
            /// <see cref="Win32.ShowWindowCommand.ShowMinimized"/>, except the 
            /// window is not activated.
            /// </summary>
            ShowMinNoActive = 7,
            /// <summary>
            /// Displays the window in its current size and position. This value is 
            /// similar to <see cref="Win32.ShowWindowCommand.Show"/>, except the 
            /// window is not activated.
            /// </summary>
            ShowNA = 8,
            /// <summary>
            /// Activates and displays the window. If the window is minimized or 
            /// maximized, the system restores it to its original size and position. 
            /// An application should specify this flag when restoring a minimized window.
            /// </summary>
            Restore = 9,
            /// <summary>
            /// Sets the show state based on the SW_* value specified in the 
            /// STARTUPINFO structure passed to the CreateProcess function by the 
            /// program that started the application.
            /// </summary>
            ShowDefault = 10,
            /// <summary>
            ///  <b>Windows 2000/XP:</b> Minimizes a window, even if the thread 
            /// that owns the window is not responding. This flag should only be 
            /// used when minimizing windows from a different thread.
            /// </summary>
            ForceMinimize = 11
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            thread.Abort();
        }
    }

    public class WpfGamepad : GamepadBase
    {
        private InputSimulator _inputSimulator;

        public WpfGamepad()
        {
            _inputSimulator = new InputSimulator();
        }

        public override void Press(Keys key)
        {
            _inputSimulator.Keyboard.KeyDown(GetVkCode(key));
        }

        public override void Release(Keys key)
        {
            _inputSimulator.Keyboard.KeyUp(GetVkCode(key));
        }

        protected VirtualKeyCode GetVkCode(Keys key)
        {
            switch (key)
            {
                case Keys.Right:
                    return VirtualKeyCode.RIGHT;
                case Keys.Left:
                    return VirtualKeyCode.LEFT;
                case Keys.Down:
                    return VirtualKeyCode.DOWN;
                case Keys.Up:
                    return VirtualKeyCode.UP;
                case Keys.Start:
                    return VirtualKeyCode.RETURN;
                default:
                    return VirtualKeyCode.CANCEL;
            }
        }
    }
}
