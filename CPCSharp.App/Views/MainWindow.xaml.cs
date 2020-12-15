using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using CPCSharp.Core;

namespace CPCSharp.App.Views
{
    public class MainWindow : Window
    {
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
            { Key.Oem4, CPCKey.AtSymbol },
            { Key.OemCloseBrackets, CPCKey.OpenSquareBracket },
            { Key.Tab, CPCKey.Tab }
        };

        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            this.KeyDown += MainWindow_KeyDown;
            this.KeyUp += MainWindow_KeyUp;
        }

        void MainWindow_KeyDown(object sender, KeyEventArgs e) {
            Console.WriteLine("Down:" + e.Key.ToString());

            if (e.KeyModifiers.HasFlag(KeyModifiers.Alt) && e.Key == Key.Return) {
                KeyboardState.Instance.KeyDown(CPCKey.Enter);
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
        void MainWindow_KeyUp(object sender, KeyEventArgs e) {
            Console.WriteLine("Up: " + e.Key.ToString());
            e.Handled = true;
            if (e.Key == Key.Return) {
                KeyboardState.Instance.KeyUp(CPCKey.Enter);
            }
            if (!e.KeyModifiers.HasFlag(KeyModifiers.Shift)) {
                KeyboardState.Instance.KeyUp(CPCKey.Shift);
            }
            if (KeyMapping.ContainsKey(e.Key)) {

                KeyboardState.Instance.KeyUp(KeyMapping[e.Key]);
            }
        }
    }
}