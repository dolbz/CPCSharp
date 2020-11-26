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
            { Key.Space, CPCKey.Space }
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
            e.Handled = true;
            if (KeyMapping.ContainsKey(e.Key)) {
                KeyboardState.Instance.KeyDown(KeyMapping[e.Key]);
            }
        }
        void MainWindow_KeyUp(object sender, KeyEventArgs e) {
            Console.WriteLine("Up: " + e.Key.ToString());
            e.Handled = true;
            if (KeyMapping.ContainsKey(e.Key)) {
                KeyboardState.Instance.KeyUp(KeyMapping[e.Key]);
            }
        }
    }
}