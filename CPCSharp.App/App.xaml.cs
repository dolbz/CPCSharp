using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CPCSharp.App.Views;
using CPCSharp.Core;
using CPCSharp.ViewModels;

namespace CPCSharp.App
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
                var debugWindow = new DebugWindow() {
                    DataContext = new DebugWindowViewModel(_runner)
                };
                debugWindow.Show();
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}