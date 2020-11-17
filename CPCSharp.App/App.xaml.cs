using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CPCSharp.Core;
using CPCSharp.ViewModels;
using CPCSharp.Views;

namespace CPCSharp
{
    public class App : Application
    {
        private CPCRunner _runner;

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            _runner = new CPCRunner();
            _runner.Initialize();
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {

                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(_runner),
                };

            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}