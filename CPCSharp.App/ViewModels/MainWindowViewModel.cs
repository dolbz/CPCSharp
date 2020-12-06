using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Timers;
using Avalonia;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CPCSharp.Core;
using ReactiveUI;
using CPCSharp.App;

namespace CPCSharp.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly CPCRunner _runner;
        private readonly ScreenRenderer _renderer;

        private Bitmap _screenBitmap; 
        public Bitmap ScreenBitmap
        {
            get => _screenBitmap;
            private set => this.RaiseAndSetIfChanged(ref _screenBitmap, value);
        }

        public MainWindowViewModel(CPCRunner runner, ScreenRenderer screenRenderer) {
            _runner = runner;
            _renderer = screenRenderer;
            screenRenderer.RegisterScreenCompleteCallback(OnUpdate);
        }

        private void OnUpdate()
        {
            ScreenBitmap = _renderer.liveBuffer;
        }
    }
}
