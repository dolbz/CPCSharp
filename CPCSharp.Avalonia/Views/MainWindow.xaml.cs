using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using CPCSharp.Core;
using CPCSharp.ViewModels;

namespace CPCSharp.App.Views
{
    public class MainWindow : Window
    {
        const double AspectRatio = 4.0 / 3.0;
        private Dictionary<Key, CPCKey> KeyMapping = new Dictionary<Key, CPCKey> {
            { Key.A, CPCKey.A },
            { Key.B, CPCKey.B },
            { Key.C, CPCKey.C },
            { Key.D, CPCKey.D },
            { Key.E, CPCKey.E },
            { Key.F, CPCKey.F },
            { Key.G, CPCKey.G },
            { Key.H, CPCKey.H },
            { Key.I, CPCKey.I },
            { Key.J, CPCKey.J },
            { Key.K, CPCKey.K },
            { Key.L, CPCKey.L },
            { Key.M, CPCKey.M },
            { Key.N, CPCKey.N },
            { Key.O, CPCKey.O },
            { Key.P, CPCKey.P },
            { Key.Q, CPCKey.Q },
            { Key.R, CPCKey.R },
            { Key.S, CPCKey.S },
            { Key.T, CPCKey.T },
            { Key.U, CPCKey.U },
            { Key.V, CPCKey.V },
            { Key.W, CPCKey.W },
            { Key.X, CPCKey.X },
            { Key.Y, CPCKey.Y },
            { Key.Z, CPCKey.Z },
            { Key.D0, CPCKey.Num0 },
            { Key.D1, CPCKey.Num1 },
            { Key.D2, CPCKey.Num2 },
            { Key.D3, CPCKey.Num3 },
            { Key.D4, CPCKey.Num4 },
            { Key.D5, CPCKey.Num5 },
            { Key.D6, CPCKey.Num6 },
            { Key.D7, CPCKey.Num7 },
            { Key.D8, CPCKey.Num8 },
            { Key.D9, CPCKey.Num9 },
            { Key.Oem2, CPCKey.ForwardSlash },
            { Key.OemBackslash, CPCKey.BackSlash },
            { Key.OemSemicolon, CPCKey.SemiColon },
            { Key.Space, CPCKey.Space },
            { Key.Return, CPCKey.Return },
            { Key.Up, CPCKey.CurUp },
            { Key.Down, CPCKey.CurDown },
            { Key.Left, CPCKey.CurLeft },
            { Key.Right, CPCKey.CurRight },
            { Key.Back, CPCKey.Delete },
            { Key.OemComma, CPCKey.Comma },
            { Key.OemPeriod, CPCKey.Period },
            { Key.Oem4, CPCKey.AtSymbol },
            { Key.OemCloseBrackets, CPCKey.OpenSquareBracket },
            { Key.Tab, CPCKey.Tab },
            { Key.OemMinus, CPCKey.Minus },
            { Key.NumPad0, CPCKey.F0 },
            { Key.NumPad1, CPCKey.F1 },
            { Key.NumPad2, CPCKey.F2 },
            { Key.NumPad3, CPCKey.F3 },
            { Key.NumPad4, CPCKey.F4 },
            { Key.NumPad5, CPCKey.F5 },
            { Key.NumPad6, CPCKey.F6 },
            { Key.NumPad7, CPCKey.F7 },
            { Key.NumPad8, CPCKey.F8 },
            { Key.NumPad9, CPCKey.F9 },
            { Key.Escape, CPCKey.Escape }
        };

        public MainWindow()
        {
            InitializeComponent();
        }

        private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.KeyDown += MainWindow_KeyDown;
            this.KeyUp += MainWindow_KeyUp;
            var clientSize = this.GetObservable(Window.ClientSizeProperty);

            var lastSize = ClientSize;
            var ignoreNextChangeEvent = false;

            clientSize.Subscribe(newSize => {
                if (ignoreNextChangeEvent) {
                    ignoreNextChangeEvent = false;
                } else {
                    var widthChangeFactor = newSize.Width/lastSize.Width;
                    var heighChangeFactor = newSize.Height/lastSize.Height;

                    var absWidthFactor = Math.Abs(widthChangeFactor-1);
                    var absHeightFactor = Math.Abs(heighChangeFactor-1);

                    if (absWidthFactor > absHeightFactor) {
                        var usedNewSize = new Size(Math.Floor(newSize.Width), Math.Floor(newSize.Width / AspectRatio));
                        ignoreNextChangeEvent = true;
                        lastSize = ClientSize;
                        ClientSize = usedNewSize;
                    } else if (absHeightFactor > absWidthFactor) {
                        var usedNewSize = new Size(Math.Floor(newSize.Height*AspectRatio), Math.Floor(newSize.Height));
                        ignoreNextChangeEvent = true;
                        lastSize = ClientSize;
                        ClientSize = usedNewSize;
                    }
                }
            });
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            ViewModel.Window = this;
        }

        public void LoadTape() {
            Console.WriteLine("Loading tape!");
        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e) {
            Console.WriteLine("Down:" + e.Key.ToString());
            Console.WriteLine("Key Mod: " + e.KeyModifiers);
            if (e.KeyModifiers.HasFlag(KeyModifiers.Alt)) {
                CPCKey key;
                switch (e.Key) {
                    case Key.Return:
                        key = CPCKey.Enter;
                        break;
                    case Key.D0:
                        key = CPCKey.F0;
                        break;
                    case Key.D1:
                        key = CPCKey.F1;
                        break;
                    case Key.D2:
                        key = CPCKey.F2;
                        break;
                    case Key.D3:
                        key = CPCKey.F3;
                        break;
                    case Key.D4:
                        key = CPCKey.F4;
                        break;
                    case Key.D5:
                        key = CPCKey.F5;
                        break;
                    case Key.D6:
                        key = CPCKey.F6;
                        break;
                    case Key.D7:
                        key = CPCKey.F7;
                        break;
                    case Key.D8:
                        key = CPCKey.F8;
                        break;
                    case Key.D9:
                        key = CPCKey.F9;
                        break;
                    case Key.OemMinus:
                        key = CPCKey.FDot;
                        break;
                    default:
                        return;
                }
                KeyboardState.Instance.KeyDown(key);
                // On mac at least there's no keyup event if this modifier is held down so we'll have to manually
                // make a keyup happen.
                e.Handled = true;
            }
            if (e.KeyModifiers.HasFlag(KeyModifiers.Control)) {
                KeyboardState.Instance.KeyDown(CPCKey.Control);
            }
            if (e.KeyModifiers.HasFlag(KeyModifiers.Shift)) {
                KeyboardState.Instance.KeyDown(CPCKey.Shift);
            }
            if (KeyMapping.ContainsKey(e.Key) && !e.Handled) {
                KeyboardState.Instance.KeyDown(KeyMapping[e.Key]);
            }
            e.Handled = true;
        }

        void ScheduleKeyUp(CPCKey key) {
            Task.Run(() => {
                Task.Delay(100).Wait();
                KeyboardState.Instance.KeyUp(key);
            });
        }

        void MainWindow_KeyUp(object sender, KeyEventArgs e) {
            Console.WriteLine("Up: " + e.Key.ToString());
            Console.WriteLine("Key Mod: " + e.KeyModifiers);
            
            if (e.KeyModifiers.HasFlag(KeyModifiers.Alt)) {
                CPCKey key;
                switch (e.Key) {
                    case Key.Return:
                        key = CPCKey.Enter;
                        break;
                    case Key.D0:
                        key = CPCKey.F0;
                        break;
                    case Key.D1:
                        key = CPCKey.F1;
                        break;
                    case Key.D2:
                        key = CPCKey.F2;
                        break;
                    case Key.D3:
                        key = CPCKey.F3;
                        break;
                    case Key.D4:
                        key = CPCKey.F4;
                        break;
                    case Key.D5:
                        key = CPCKey.F5;
                        break;
                    case Key.D6:
                        key = CPCKey.F6;
                        break;
                    case Key.D7:
                        key = CPCKey.F7;
                        break;
                    case Key.D8:
                        key = CPCKey.F8;
                        break;
                    case Key.D9:
                        key = CPCKey.F9;
                        break;
                    case Key.OemMinus:
                        key = CPCKey.FDot;
                        break;
                    default:
                        return;
                }
                KeyboardState.Instance.KeyUp(key);
                // On mac at least there's no keyup event if this modifier is held down so we'll have to manually
                // make a keyup happen.
                e.Handled = true;
            }
            if (!e.KeyModifiers.HasFlag(KeyModifiers.Control)) {
                KeyboardState.Instance.KeyUp(CPCKey.Shift);
            }
            if (!e.KeyModifiers.HasFlag(KeyModifiers.Shift)) {
                KeyboardState.Instance.KeyUp(CPCKey.Shift);
            }
            if (KeyMapping.ContainsKey(e.Key) && !e.Handled) {

                KeyboardState.Instance.KeyUp(KeyMapping[e.Key]);
            }
            e.Handled = true;
        }
    }
}