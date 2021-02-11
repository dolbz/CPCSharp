namespace CPCSharp.Core.PSG {

    /// <summary>
    /// Interface to platform specific PSG code to generate audio
    /// </summary>
    public interface INativePSG {
        void SetTone(PSGChannel channel, float frequency);

        void SetAmplitudeMode(PSGChannel channel, bool isFixedMode);

        void SetAmplitude(PSGChannel channel, float amplitude);

        void SetChannelAttributes(PSGChannel channel, bool channelEnabled, bool noiseOn);

        void SetNoiseFrequency(float frequency);

        void SetEnvelopeFrequency(float frequency);

        void SetEnvelopeShape(EnvelopeShape envelope);
    }
}