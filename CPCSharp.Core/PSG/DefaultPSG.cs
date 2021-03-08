namespace CPCSharp.Core.PSG
{
    /// <summary>
    /// Default no-op implementation for platforms without a native PSG implementation
    /// </summary>
    public class DefaultPSG : INativePSG
    {
        public void SetAmplitude(PSGChannel channel, float amplitude)
        {
        }

        public void SetAmplitudeMode(PSGChannel channel, bool isFixedMode)
        {
        }

        public void SetChannelAttributes(PSGChannel channel, bool channelEnabled, bool noiseOn)
        {
        }

        public void SetEnvelopeFrequency(float frequency)
        {
        }

        public void SetEnvelopeShape(EnvelopeShape envelope)
        {
        }

        public void SetNoiseFrequency(float frequency)
        {
        }

        public void SetTone(PSGChannel channel, float frequency)
        {
        }
    }
}
