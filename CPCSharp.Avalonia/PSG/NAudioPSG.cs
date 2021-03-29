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
    /// <summary>
    /// Adapted version of the SignalGenerator class in NAudio. For code from NAudio the following copyright notice/license applies:
    /// 
    /// Copyright 2020 Mark Heath
    /// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation 
    /// files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, 
    /// modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software 
    /// is furnished to do so, subject to the following conditions:
    /// 
    /// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
    /// 
    /// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES 
    /// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE 
    /// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR 
    /// IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
    /// </summary>
    public class AyChannelSignalGenerator : ISampleProvider
    {
        // Wave format
        private readonly WaveFormat waveFormat;

        // Generator variable
        private int nSample;

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

        private int _noisePeriod = 1;
        private int _noiseCount = 0;

        public double NoiseFrequency
        {
            set
            {
                if (value == 0) {
                    _noisePeriod = 1;
                }
                else
                {
                    _noisePeriod = (int)(waveFormat.SampleRate / value);
                }
                _noiseCount = 0;
            }
        }

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

        private int _noiseSeed = 0xffff;

        private bool NextNoiseBit
        {
            get
            {
                _noiseSeed ^= (((_noiseSeed & 1) ^ ((_noiseSeed >> 3) & 1)) << 17);
                _noiseSeed >>= 1;

                return CurrentNoiseBit;
            }
        }

        private bool CurrentNoiseBit
        {
            get
            {
                return (_noiseSeed & 1) == 1;
            }
        }

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
                var noiseBit = CurrentNoiseBit;
                if (_noiseCount == _noisePeriod) {
                    var nextNoiseBit = NextNoiseBit;
                    if (NoiseEnabled)
                    {
                        noiseBit = nextNoiseBit;
                    }
                }

                // Square Generator
                multiple = 2 * Frequency / waveFormat.SampleRate;
                sampleSaw = ((nSample * multiple) % 2) - 1;
                sampleValue = sampleSaw > 0 ? Gain : -Gain;


                if ((!ToneEnabled || sampleSaw > 0) && (!NoiseEnabled || noiseBit)) {
                    sampleValue = Gain;
                } else
                {
                    sampleValue = -Gain;
                 }


                _noiseCount += 1;
                if (_noiseCount > _noisePeriod)
                {
                    _noiseCount = 0;
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
            GetObject(channel).NoiseEnabled = noiseOn;
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
            // TODO there's certainly a nicer way to tie these together than this
            _channelA.NoiseFrequency = frequency;
            _channelB.NoiseFrequency = frequency;
            _channelC.NoiseFrequency = frequency;
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
