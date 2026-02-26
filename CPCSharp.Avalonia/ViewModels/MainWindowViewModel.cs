//  
// Copyright (c) 2021, Nathan Randle. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
﻿using System;
using Avalonia.Media.Imaging;
using CPCSharp.Core;
using ReactiveUI;
using CPCSharp.App;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Collections.Generic;

namespace CPCSharp.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly CPCRunner _runner;
        private readonly ScreenRenderer _renderer;

        public Window? Window { get; set; }

        private Bitmap? _screenBitmap;
        public Bitmap? ScreenBitmap
        {
            get => _screenBitmap;
            private set => this.RaiseAndSetIfChanged(ref _screenBitmap, value);
        }

        private int _width;
        public int Width 
        { 
            get => _width; 
            private set => this.RaiseAndSetIfChanged(ref _width, value); 
        }

        private int _height;
        public int Height 
        { 
            get => _height; 
            private set => this.RaiseAndSetIfChanged(ref _height, value); 
        }

        public MainWindowViewModel(CPCRunner runner, ScreenRenderer screenRenderer) {
            _runner = runner;
            _renderer = screenRenderer;
            screenRenderer.RegisterScreenCompleteCallback(OnUpdate);
        }

        public async void LoadTape() {
            var storageProvider = (Window ?? throw new InvalidOperationException("Window property accessed before it has been set")).StorageProvider;

            var files = await storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = "Open file",
                FileTypeFilter = new List<FilePickerFileType> {
                    new FilePickerFileType("CDT Files") { Patterns = new List<string> { "*.cdt" } }
                }
            });

            if (files?.Count > 0) {
                var path = files[0].Path.LocalPath;
                Console.WriteLine($"Chose file {path}");
                _runner.LoadTape(path);
            }
        }

        public void Reset() 
        {
            _runner.Reset();
        }

        private void OnUpdate()
        {
            ScreenBitmap = _renderer.ScreenBuffer;
            Width = (int)_renderer.ScreenBuffer.Size.Width;
            Height = (int)_renderer.ScreenBuffer.Size.Height;
        }
    }
}
