using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CPCSharp.App.Views;
using CPCSharp.Core;
using CPCSharp.Core.Interfaces;
using CPCSharp.ViewModels;

namespace CPCSharp.App
{
    public class App : Application
    {
        private CPCRunner _runner;
        private ScreenRenderer _renderer;

        public override void Initialize()
        {
            _renderer = new ScreenRenderer();
            AvaloniaXamlLoader.Load(this);
            _runner = new CPCRunner(_renderer);
            _runner.Initialize();
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {

                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(_runner, _renderer),
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