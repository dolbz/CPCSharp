using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using CPCSharp.App.Views;
using CPCSharp.Core;
using CPCSharp.Core.PSG;
using CPCSharp.ViewModels;

namespace CPCSharp.App
{
    public class App : Application
    {
        private CPCRunner _runner;
        private ScreenRenderer _renderer;

        public override void Initialize()
        {
            var os = AvaloniaLocator.Current.GetService<IRuntimePlatform>().GetRuntimeInfo().OperatingSystem;

            INativePSG psg = new DefaultPSG();
            switch (os)
            {
                case OperatingSystemType.OSX:
                    psg = new MacPSGInterop();
                    break;
            }
            _renderer = new ScreenRenderer();
            AvaloniaXamlLoader.Load(this);
            _runner = new CPCRunner(_renderer, psg);
            _runner.Initialize();
            
            var args = Environment.GetCommandLineArgs();

            const string TapeArg = "--tape=";
            foreach (var arg in args) {
                if (arg.StartsWith(TapeArg)) {
                    var tapePath = arg.Split('=')[1];
                    _runner.LoadTape(tapePath);
                }
            }

            if (Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                desktopLifetime.Exit += OnExit;
            }
        }

        private void OnExit(object sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            _runner.Shutdown();
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