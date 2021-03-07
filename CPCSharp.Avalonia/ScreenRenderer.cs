using System.Security.Cryptography;
using System.Drawing;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CPCSharp.Core.Interfaces;
using System.Runtime.InteropServices;
using System;

namespace CPCSharp.App {
    public class ScreenRenderer : IScreenRenderer {
        private WriteableBitmap workingBuffer;
        public WriteableBitmap ScreenBuffer { get; set; }
        private int[] data = new int[832*600]; // 832x288 Resolution is max possible so we've got enough memory in all cases without reallocating

        private Action _screenCompleteCallBack;

        public System.Drawing.Size ScreenDimensions { get; private set; }

        private int dataIndex = 0;
        private bool waitForHSync = false;

        public ScreenRenderer() {
            workingBuffer = new WriteableBitmap(new PixelSize(320, 200), new Vector(96, 96), PixelFormat.Bgra8888);
            ScreenBuffer = new WriteableBitmap(new PixelSize(320, 200), new Vector(96, 96), PixelFormat.Bgra8888);
            ScreenDimensions = new System.Drawing.Size(320, 200);
        }

        public void SendHsyncEnd()
        {
            // TODO I don't think a real monitor waits for the HSYNC after VSYNC like this but It's the only way 
            // I could get things to layout nicely. I need to come back to this and see if I can work out how it 
            // should work...
            if (waitForHSync) {
                waitForHSync = false;

                using (var fb = workingBuffer.Lock()) {
                    Marshal.Copy(data, 0, fb.Address, fb.Size.Width * fb.Size.Height); // TODO make offset generic - 5 crtc chars for border each side * 2 cycles per char * 4 pixels per byte * 4 bytes per int for IntPtr 
                }

                dataIndex = 0;

                var oldLive = ScreenBuffer;
                ScreenBuffer = workingBuffer;
                workingBuffer = oldLive;
                if (ScreenDimensions.Width != workingBuffer.Size.Width || ScreenDimensions.Height != workingBuffer.Size.Height) {
                    workingBuffer = new WriteableBitmap(new PixelSize(ScreenDimensions.Width, ScreenDimensions.Height), new Vector(96, 96), PixelFormat.Bgra8888);
                }
                _screenCompleteCallBack?.Invoke();
            }
        }

        public void SendHsyncStart()
        {
            
        }

        public void RegisterScreenCompleteCallback(Action callback) {
            _screenCompleteCallBack = callback;
        }

        public void SendPixels(Color[] pixels)
        {
            foreach (var pixel in pixels) {
                data[dataIndex++] = (int)Avalonia.Media.Color.FromRgb(pixel.R, pixel.G, pixel.B).ToUint32();
            }
        }

        public void SendVsyncEnd()
        {
            waitForHSync = true;
        }

        public void SendVsyncStart()
        {
            // TODO
        }

        public void ResolutionChanged(System.Drawing.Size dimensions)
        {
            ScreenDimensions = dimensions;
        }
    }
}