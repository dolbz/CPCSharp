//  
// Copyright (c) 2021, Nathan Randle. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
//

using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Rendering;
using CPCSharp.App.PSG;
using CPCSharp.App.Views;
using CPCSharp.Core;
using CPCSharp.Core.PSG;
using CPCSharp.ViewModels;
using System.Linq;
using System.Xml.Linq;

namespace CPCSharp.App
{
    public class App : Application
    {
        public CPCRunner Runner { get; private set; }
        private ScreenRenderer _renderer;

        public override void Initialize()
        {
            Name = "CPC#";

            var os = AvaloniaLocator.Current.GetService<IRuntimePlatform>().GetRuntimeInfo().OperatingSystem;

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

            _renderer = new ScreenRenderer();
            AvaloniaXamlLoader.Load(this);
            
            Runner = new CPCRunner(_renderer, psg);
            Runner.Initialize(ThreadRunMode.CycleCounted);

            var renderLoop = AvaloniaLocator.Current.GetService<IRenderLoop>();
            renderLoop.Add(new CPCRenderLoopTask(Runner));
            
            var args = Environment.GetCommandLineArgs();

            const string TapeArg = "--tape";

            for (int i = 0; i < args.Length; i++) {
                if (args[i] == TapeArg) {
                    var tapePath = args[++i];
                    Runner.LoadTape(tapePath);
                }
            }

            if (Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {
                desktopLifetime.Exit += OnExit;
            }
        }

        public void OpenAbout(object sender, EventArgs args)
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                var dialog = new AboutDialog();
                dialog.ShowDialog(desktop.MainWindow);
            }
        }

        private void OnExit(object sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            Runner.Shutdown();
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(Runner, _renderer),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}