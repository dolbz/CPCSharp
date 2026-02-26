//  
// Copyright (c) 2021, Nathan Randle. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
//

using System;
using System.Diagnostics;
using System.Threading;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CPCSharp.App.PSG;
using CPCSharp.App.Views;
using CPCSharp.Core;
using CPCSharp.Core.PSG;
using CPCSharp.ViewModels;
using System.Linq;
using System.Xml.Linq;
using Avalonia.Controls;

namespace CPCSharp.App
{
    public class App : Application
    {
        public CPCRunner Runner { 
            get => field ?? throw new InvalidOperationException("Runner accessed before it has been set"); 
            private set; 
        }

        private ScreenRenderer Renderer {
            get => field ?? throw new InvalidOperationException("Renderer accessed before it has been set");  
            set; 
        }

        public override void Initialize()
        {
            Name = "CPC#";

            INativePSG psg;

#if WINDOWS
            psg = new NAudioPSG();
#elif MACOS
            XDocument doc = XDocument.Load("/System/Library/CoreServices/SystemVersion.plist");
            var keyValues = doc.Descendants("dict")
            .SelectMany(d => d.Elements("key").Zip(d.Elements().Where(e => e.Name != "key"), (k, v) => new { Key = k, Value = v }))
            .ToDictionary(i => i.Key.Value, i => i.Value.Value);

            var rawProductVersion = keyValues["ProductVersion"];
            var productVersionParts = rawProductVersion.Split(".").Select(x => int.Parse(x)).ToArray();

            if (productVersionParts[0] > 10 || (productVersionParts[0] == 10 && productVersionParts[1] >= 15)) {
                psg = new MacPSGInterop();
            } else {
                psg = new DefaultPSG();
            }
#else
            psg = new DefaultPSG();
#endif

            Renderer = new ScreenRenderer();
            AvaloniaXamlLoader.Load(this);
            
            Runner = new CPCRunner(Renderer, psg);
            Runner.Initialize(ThreadRunMode.CycleCounted);

            var timingThread = new Thread(() =>
            {
                var sw = Stopwatch.StartNew();
                var lastTime = sw.Elapsed;
                while (true)
                {
                    Thread.Sleep(10);
                    var now = sw.Elapsed;
                    var diff = now - lastTime;
                    lastTime = now;

                    // There are 10,000,000 ticks in a second
                    // The CPC system clock is 16MHz so 16,000,000 clock pulses/second
                    // To calculate clock cycles needed we can use 1.6 * elapsed ticks 
                    // This might be slightly lossy as we don't adjust for rounding over time...
                    Runner.DispatchCycleCountRequest((int)(1.6 * diff.Ticks));
                }
            }) { IsBackground = true };
            timingThread.Start();
            
            var args = Environment.GetCommandLineArgs();

            const string TapeArg = "--tape";

            for (int i = 0; i < args.Length; i++) {
                if (args[i] == TapeArg) {
                    var tapePath = args[++i];
                    Runner.LoadTape(tapePath);
                }
            }

            if (Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                desktopLifetime.Exit += OnExit;
            }
        }

        public void OpenAbout(object sender, EventArgs args)
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                var dialog = new AboutDialog();
                dialog.ShowDialog(desktop.MainWindow ?? throw new InvalidOperationException("No main window on desktop instance"));
            }
        }

        private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            Runner.Shutdown();
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(Runner, Renderer),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}