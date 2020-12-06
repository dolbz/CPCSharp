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
        public WriteableBitmap liveBuffer;
        private int[] data = new int[320 * 200]; // TODO we don't want 320x200 hardcoded

        private Action _screenCompleteCallBack;

        private int dataIndex = 0;

        public ScreenRenderer() {
            workingBuffer = new WriteableBitmap(new PixelSize(320, 200), new Vector(96, 96), PixelFormat.Bgra8888);
            liveBuffer = new WriteableBitmap(new PixelSize(320, 200), new Vector(96, 96), PixelFormat.Bgra8888);
        }

        public void SendHsyncEnd()
        {
            
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
                if (dataIndex < data.Length) { // TODO Why do we need this if?
                    data[dataIndex++] = (int)Avalonia.Media.Color.FromRgb(pixel.R, pixel.G, pixel.B).ToUint32(); // TODO should this be casted to int?
                }
            }
        }

        public void SendVsyncEnd()
        {
            using (var fb = workingBuffer.Lock()) {
                Marshal.Copy(data, 0, fb.Address, fb.Size.Width * fb.Size.Height);
            }

            dataIndex = 0;

            var oldLive = liveBuffer;
            liveBuffer = workingBuffer;
            workingBuffer = oldLive;
            _screenCompleteCallBack?.Invoke();
        }

        public void SendVsyncStart()
        {
            // TODO
        }
    }
}