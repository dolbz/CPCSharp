using System;
using System.Runtime.InteropServices;

namespace CPCSharp.Core.PSG
{
    public class NativePSG : INativePSG {

        [DllImport("libMacPSG.dylib")]
        public static extern void SetToneNative(PSGChannel channel, float frequency);

        [DllImport("libMacPSG.dylib")]
        public static extern void SetChannelAttributesNative(PSGChannel channel, bool channelEnabled, bool noiseOn);

        [DllImport("libMacPSG.dylib")]
        public static extern void SetAmplitudeModeNative(PSGChannel channel, bool isFixedMode);

        [DllImport("libMacPSG.dylib")]
        public static extern void SetAmplitudeNative(PSGChannel channel, float amplitude);

        public void SetTone(PSGChannel channel, float frequency) {
            Console.WriteLine($"Setting tone for {channel}: {frequency}Hz");
            SetToneNative(channel, frequency);
        }

        public void SetAmplitudeMode(PSGChannel channel, bool isFixedMode) {
            Console.WriteLine($"Setting amplitude mode for {channel}: { (isFixedMode ? "Fixed Mode" : "Envelope Mode") }");
            SetAmplitudeModeNative(channel, isFixedMode);
        }

        public void SetAmplitude(PSGChannel channel, float amplitude) {
            Console.WriteLine($"Setting amplitude for {channel}: {amplitude}");
            SetAmplitudeNative(channel, amplitude);
        }

        public void SetChannelAttributes(PSGChannel channel, bool channelEnabled, bool noiseOn)
        {
            Console.WriteLine($"Setting attributes for {channel}, Enabled: {channelEnabled}, Noise On: {noiseOn}");
            SetChannelAttributesNative(channel, channelEnabled, noiseOn);
        }
    }
}