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
            KeyboardState.Instance.KeyDown(CPCKey.A);
        }
        void MainWindow_KeyUp(object sender, KeyEventArgs e) {
            Console.WriteLine("Up: " + e.Key.ToString());
            e.Handled = true;
            KeyboardState.Instance.KeyUp(CPCKey.A);
        }
    }
}