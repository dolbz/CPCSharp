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

namespace CPCSharp.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private CPCRunner _runner;

        private Bitmap _screenBitmap; 
        public Bitmap ScreenBitmap
        {
            get => _screenBitmap;
            private set => this.RaiseAndSetIfChanged(ref _screenBitmap, value);
        }

        public MainWindowViewModel(CPCRunner runner) {
            _runner = runner;
            _runner.SetRendererListener(RenderCpuScreen);
        }

        private void RenderCpuScreen() {
            _runner.AccessCpuState((cpu, ram) => {
                var data = new Color[320,200];

                data = GenerateScreenFromRam(ram);
                UpdateScreen(data);
            });
        }

        public Color[,] GenerateScreenFromRam(byte[] ram) {
            // Hardcoded Mode 1 for now

            //Address=Base + Offset + N*Column + 80*Row + 2048 per scan line. 
            // where N is the number of bits per pixel in the current mode.
            // For a given scan line, the bits are taken in sequence. 
            // The next Scan line is located by increasing the addresses by 0800.
            
            var palette = new Color[4];
            palette[0] = Color.FromRgb(0,0,255);
            palette[1] = Color.FromRgb(255,0,0);
            palette[2] = Color.FromRgb(255,255,0);
            palette[3] = Color.FromRgb(0,255,255);

            var data = new Color[320, 200];

            for (int i = 0; i < 200; i++) { // 200 scan lines in all modes 
                var lineAddress = 0xc000 + ((i / 8) * 80) + ((i % 8) * 2048);
                for (int j = 0; j < 80; j++) {
                    var ramData = ram[lineAddress+j];
                    for (int px = 0; px < 4; px++) {
                        var pixelValue = ExtractPixelValueFromRamByteMode1(ramData, px);
                        data[(j*4)+px,i] = palette[pixelValue];
                    }
                }
            }
            return data;
        }

        /// <summary>
        /// Each byte in ram represents multiple drawn pixels.
        /// In mode 1 each byte represents 4 pixels.
        /// </summary>
        /// <param name="rawRam"></param>
        /// <param name="pixelNum"></param>
        /// <returns></returns>
        public int ExtractPixelValueFromRamByteMode1(byte rawRam, int pixelNum) {
            var highNibble = rawRam >> 4;
            var lowNibble = rawRam & 0xf;

            var shiftDown = 3 - pixelNum;

            var lowBit = (lowNibble >> shiftDown) & 0x1;
            var highBit = (highNibble >> shiftDown) & 0x1;

            var pixelValue = (highBit << 1) | lowBit;
            return pixelValue;
        }

        public void UpdateScreen(Color[,] pixels) {
            var screen = new WriteableBitmap(new PixelSize(320, 200), new Vector(96, 96), PixelFormat.Bgra8888);

            using(var fb = screen.Lock()) {
                var data = new int[fb.Size.Width * fb.Size.Height];

                for (int y = 0; y < fb.Size.Height; y++)
                {
                    for (int x = 0; x < fb.Size.Width; x++)
                    {
                        data[y * fb.Size.Width + x] = (int)pixels[x,y].ToUint32();
                    }
                }
                Marshal.Copy(data, 0, fb.Address, fb.Size.Width * fb.Size.Height);
            }

            ScreenBitmap = screen;
        }
    }
}
