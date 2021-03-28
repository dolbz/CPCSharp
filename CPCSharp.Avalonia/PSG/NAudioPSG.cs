//  
// Copyright (c) 2021, Nathan Randle. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 

#if WINDOWS
using CPCSharp.Core.PSG;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;

namespace CPCSharp.App.PSG
{
    public class AyChannelSignalGenerator : ISampleProvider
    {
        // Wave format
        private readonly WaveFormat waveFormat;

        // Random Number for the White Noise & Pink Noise Generator
        private readonly Random random = new Random();

        private readonly double[] pinkNoiseBuffer = new double[7];

        // Const Math
        private const double TwoPi = 2 * Math.PI;

        // Generator variable
        private int nSample;

        // Sweep Generator variable
        private double phi;

        /// <summary>
        /// Initializes a new instance for the Generator (Default :: 44.1Khz, 2 channels, Sinus, Frequency = 440, Gain = 1)
        /// </summary>
        public AyChannelSignalGenerator()
            : this(44100, 2)
        {

        }

        /// <summary>
        /// Initializes a new instance for the Generator (UserDef SampleRate &amp; Channels)
        /// </summary>
        /// <param name="sampleRate">Desired sample rate</param>
        /// <param name="channel">Number of channels</param>
        public AyChannelSignalGenerator(int sampleRate, int channel)
        {
            phi = 0;
            waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channel);

            // Default
            Frequency = 440.0;
            Gain = 1;
            PhaseReverse = new bool[channel];
            SweepLengthSecs = 2;
        }

        /// <summary>
        /// The waveformat of this WaveProvider (same as the source)
        /// </summary>
        public WaveFormat WaveFormat => waveFormat;

        /// <summary>
        /// Frequency for the Generator. (20.0 - 20000.0 Hz)
        /// Sin, Square, Triangle, SawTooth, Sweep (Start Frequency).
        /// </summary>
        public double Frequency { get; set; }

        /// <summary>
        /// Return Log of Frequency Start (Read only)
        /// </summary>
        public double FrequencyLog => Math.Log(Frequency);

        /// <summary>
        /// End Frequency for the Sweep Generator. (Start Frequency in Frequency)
        /// </summary>
        public double FrequencyEnd { get; set; }

        /// <summary>
        /// Return Log of Frequency End (Read only)
        /// </summary>
        public double FrequencyEndLog => Math.Log(FrequencyEnd);

        /// <summary>
        /// Gain for the Generator. (0.0 to 1.0)
        /// </summary>
        public double Gain { get; set; }

        public bool ToneEnabled { get; set; }

        public bool NoiseEnabled { get; set; }

        /// <summary>
        /// Channel PhaseReverse
        /// </summary>
        public bool[] PhaseReverse { get; }

        /// <summary>
        /// Length Seconds for the Sweep Generator.
        /// </summary>
        public double SweepLengthSecs { get; set; }

        /// <summary>
        /// Reads from this provider.
        /// </summary>
        public int Read(float[] buffer, int offset, int count)
        {
            int outIndex = offset;

            // Generator current value
            double multiple;
            double sampleValue;
            double sampleSaw;

            // Complete Buffer
            for (int sampleCount = 0; sampleCount < count / waveFormat.Channels; sampleCount++)
            {
                // Square Generator
                multiple = 2 * Frequency / waveFormat.SampleRate;
                sampleSaw = ((nSample * multiple) % 2) - 1;
                sampleValue = sampleSaw > 0 ? Gain : -Gain;

                if (!ToneEnabled)
                {
                    sampleValue = Gain;
                }

                nSample++;

                // Phase Reverse Per Channel
                for (int i = 0; i < waveFormat.Channels; i++)
                {
                    if (PhaseReverse[i])
                        buffer[outIndex++] = (float)-sampleValue;
                    else
                        buffer[outIndex++] = (float)sampleValue;
                }
            }
            return count;
        }

        /// <summary>
        /// Private :: Random for WhiteNoise &amp; Pink Noise (Value form -1 to 1)
        /// </summary>
        /// <returns>Random value from -1 to +1</returns>
        private double NextRandomTwo()
        {
            return 2 * random.NextDouble() - 1;
        }

    }

    public class NAudioPSG : INativePSG
    {
        private readonly AyChannelSignalGenerator _channelA;
        private readonly AyChannelSignalGenerator _channelB;
        private readonly AyChannelSignalGenerator _channelC;
        private readonly WasapiOut _wasapiOut;

        public NAudioPSG()
        {
            _channelA = new AyChannelSignalGenerator()
            {
                ToneEnabled = false
            };

            _channelB = new AyChannelSignalGenerator()
            {
                ToneEnabled = false
            };

            _channelC = new AyChannelSignalGenerator()
            {
                ToneEnabled = false
            };

            var mix = new MixingSampleProvider(new[] { _channelA, _channelB, _channelC });
            
            _wasapiOut = new WasapiOut();
            _wasapiOut.Init(mix);
            _wasapiOut.Play();
        }

        public AyChannelSignalGenerator GetObject(PSGChannel channel)
        {
            switch (channel)
            {
                case PSGChannel.A:
                    return _channelA;
                case PSGChannel.B:
                    return _channelB;
                case PSGChannel.C:
                    return _channelC;
                default:
                    throw new InvalidOperationException("Unknown channel selected");
            }
        }

        public void SetAmplitude(PSGChannel channel, float amplitude)
        {
            GetObject(channel).Gain = amplitude;
        }

        public void SetAmplitudeMode(PSGChannel channel, bool isFixedMode)
        {
            //throw new System.NotImplementedException();
        }

        public void SetChannelAttributes(PSGChannel channel, bool channelEnabled, bool noiseOn)
        {
            GetObject(channel).ToneEnabled = channelEnabled;
        }

        public void SetEnvelopeFrequency(float frequency)
        {
            //throw new System.NotImplementedException();
        }

        public void SetEnvelopeShape(EnvelopeShape envelope)
        {
            //throw new System.NotImplementedException();
        }

        public void SetNoiseFrequency(float frequency)
        {
            // TODO noise
        }

        public void SetTone(PSGChannel channel, float frequency)
        {
            GetObject(channel).Frequency = frequency;
        }

        public void Shutdown()
        {
            _wasapiOut.Stop();
        }
    }
}
#endif
