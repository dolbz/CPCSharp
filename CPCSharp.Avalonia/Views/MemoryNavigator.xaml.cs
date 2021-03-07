using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CPCSharp.App.Views
{
    public class MemoryNavigator : Window
    {
        public MemoryNavigator()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}