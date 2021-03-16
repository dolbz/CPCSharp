using System;
using Avalonia.Media.Imaging;
using CPCSharp.Core;
using ReactiveUI;
using CPCSharp.App;
using Avalonia.Controls;
using System.Collections.Generic;

namespace CPCSharp.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly CPCRunner _runner;
        private readonly ScreenRenderer _renderer;

        public Window Window { get; set; }

        private Bitmap _screenBitmap; 
        public Bitmap ScreenBitmap
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
            var openDialog = new OpenFileDialog()
            {
                Title = "Open file",
                Filters = new List<FileDialogFilter> {
                    new FileDialogFilter { Name = "CDT Files", Extensions = new List<string> { "cdt" } }
                }
            };
                
            var chosenFile = await openDialog.ShowAsync(Window);
            
            if (chosenFile.Length > 0) {
                Console.WriteLine($"Chose file {chosenFile[0]}");
                _runner.LoadTape(chosenFile[0]);
            }
        }

        private void OnUpdate()
        {
            ScreenBitmap = _renderer.ScreenBuffer;
            Width = (int)_renderer.ScreenBuffer.Size.Width;
            Height = (int)_renderer.ScreenBuffer.Size.Height;
        }
    }
}
