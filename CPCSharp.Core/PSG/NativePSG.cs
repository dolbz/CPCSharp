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

        [DllImport("libMacPSG.dylib")]
        public static extern void SetNoiseFrequencyNative(float frequency);

        [DllImport("libMacPSG.dylib")]
        public static extern void SetEnvelopeFrequencyNative(float frequency);

        [DllImport("libMacPSG.dylib")]
        public static extern void SetEnvelopeShapeNative(EnvelopeShape envelope);

        public void SetTone(PSGChannel channel, float frequency) {
            SetToneNative(channel, frequency);
        }

        public void SetAmplitudeMode(PSGChannel channel, bool isFixedMode) {
            SetAmplitudeModeNative(channel, isFixedMode);
        }

        public void SetAmplitude(PSGChannel channel, float amplitude) {
            SetAmplitudeNative(channel, amplitude);
        }

        public void SetChannelAttributes(PSGChannel channel, bool channelEnabled, bool noiseOn)
        {
            SetChannelAttributesNative(channel, channelEnabled, noiseOn);
        }

        public void SetNoiseFrequency(float frequency) {
            SetNoiseFrequencyNative(frequency);
        }

        public void SetEnvelopeFrequency(float frequency) {
            SetEnvelopeFrequencyNative(frequency);
        }

        public void SetEnvelopeShape(EnvelopeShape envelope) {
            SetEnvelopeShapeNative(envelope);
        }
    }
}