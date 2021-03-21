using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace CPCSharp.App.Views
{
    public class LegalBitsDialog : Window
    {
        public LegalBitsDialog()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}