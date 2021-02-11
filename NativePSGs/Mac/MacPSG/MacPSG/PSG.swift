//
//  PSG.swift
//  MacPSG
//
//  Created by Nathan Randle on 02/02/2021.
//

import AVFoundation
import Foundation

typealias Signal = (_ frequency: Float, _ time: Float) -> Float

enum Waveform: Int {
    case sine, triangle, sawtooth, square, whiteNoise
}

struct Envelope {
    static let fadeDownOnceHoldLow: Signal = { frequency, time in
        let period = 1.0 / (frequency * 2)
        let localTime = fmod(time, period)
        
        if time > period {
            return 0
        } else {
            return 1 - (Float(localTime) / period)
        }
    }
    
    static let fadeUpOnceHoldLow: Signal = { frequency, time in
        let period = 1.0 / (frequency * 2)
        let localTime = fmod(time, period)
        
        if time > period {
            return 0
        } else {
            return Float(localTime) / period
        }
    }
    
    static let fadeDownOnceHoldHigh: Signal = { frequency, time in
        let period = 1.0 / (frequency * 2)
        let localTime = fmod(time, period)
        
        if time > period {
            return 1
        } else {
            return 1 - (Float(localTime) / period)
        }
    }
    
    static let fadeUpOnceHoldHigh: Signal = { frequency, time in
        let period = 1.0 / (frequency * 2)
        let localTime = fmod(time, period)
        
        if time > period {
            return 1
        } else {
            return Float(localTime) / period
        }
    }
    
    static let descendingSawTooth: Signal = { frequency, time in
        let period = 1.0 / (frequency * 2)
        let currentTime = fmod(Double(time), Double(period))
        return 1 - (Float(currentTime) / period)
    }
    
    static let ascendingSawTooth: Signal = { frequency, time in
        let period = 1.0 / (frequency * 2)
        let currentTime = fmod(Double(time), Double(period))
        return Float(currentTime) / period
    }
    
    static let ascendingTriangle: Signal = { frequency, time in
        let period = 1.0 / frequency;
        let halfPeriod = period / 2.0 ;
        let currentTime = Float(fmod(Double(time), Double(period)))
        
        if (currentTime <= halfPeriod) {
            return Float(currentTime) / halfPeriod
        } else {
            return 1 - ((currentTime - halfPeriod) / halfPeriod)
        }
    }
    
    static let descendingTriangle: Signal = { frequency, time in
        let period = 1.0 / frequency;
        let halfPeriod = period / 2.0 ;
        let currentTime = Float(fmod(Double(time), Double(period)))
        
        if (currentTime > halfPeriod) {
            return Float(currentTime) / halfPeriod
        } else {
            return 1 - ((currentTime - halfPeriod) / halfPeriod)
        }
    }
}

struct Oscillator {
    
    static var amplitude: Float = 1
    
    static let sine: Signal = { frequency, time in
        return Oscillator.amplitude * sin(2.0 * Float.pi * frequency * time)
    }
    
    static let triangle: Signal = { frequency, time in
        let period = 1.0 / Double(frequency)
        let currentTime = fmod(Double(time), period)
        
        let value = currentTime / period
        
        var result = 0.0
        if value < 0.25 {
            result = value * 4
        } else if value < 0.75 {
            result = 2.0 - (value * 4.0)
        } else {
            result = value * 4 - 4.0
        }
        
        return Oscillator.amplitude * Float(result)
    }

    static let sawtooth: Signal = { frequency, time in
        let period = 1.0 / frequency
        let currentTime = fmod(Double(time), Double(period))
        return Oscillator.amplitude * ((Float(currentTime) / period) * 2 - 1.0)
    }
    
    static let square: Signal = { frequency, time in
        let period = 1.0 / Double(frequency)
        let currentTime = fmod(Double(time), period)
        return ((currentTime / period) < 0.5) ? Oscillator.amplitude : -1.0 * Oscillator.amplitude
    }
    
    static let whiteNoise: Signal = { frequency, time in
        return Oscillator.amplitude * Float.random(in: -1...1)
    }
}

@objc
class ChannelGenerator : NSObject {
    @objc
    public var amplitudeFixedMode: Bool = true
    
    @objc
    public var toneEnabled: Bool = true // TODO defaults?
    
    @objc
    public var noiseEnabled: Bool = true
    
    @objc
    public var amplitude: Float = 1
    
    @objc
    public var frequency: Float = 1 {
        didSet {
            if frequency == 0 {
                self.tonePeriod = 1
            } else {
                self.tonePeriod = Int(Float(sampleRate)/frequency)
            }
            self.toneCount = 0
        }
    }
    
    public var noiseFrequency: Float = 1 {
        didSet {
            if noiseFrequency == 0 {
                self.noisePeriod = 1
            } else {
                self.noisePeriod = Int(Float(sampleRate)/noiseFrequency)
            }
            self.noiseCount = 0
        }
    }
    
    public var envelopeFrequency: Float = 1 {
        didSet {
            if envelopeFrequency == 0 {
                self.envelopePeriod = 1
            } else {
                self.envelopePeriod = Int(Float(sampleRate)/envelopeFrequency)
            }
            self.envelopeCount = 0
        }
    }
    
    public var envelope: Signal = Envelope.fadeDownOnceHoldLow
    
    private var tonePeriod: Int = 1
    private var toneCount: Int = 0
    
    private var noisePeriod: Int = 1
    private var noiseCount: Int = 0
    private var noiseSeed: Int = 0xffff
    
    private var envelopePeriod: Int = 1
    private var envelopeCount: Int = 0
    
    private var sampleRate: Int
    
    init(sampleRate: Int) {
        self.sampleRate = sampleRate
        super.init()
    }
    
    private var lastNoiseBit: Bool = false
    private var nextNoiseBit: Bool {
        get {
            self.noiseSeed ^= (((self.noiseSeed & 1) ^ ((self.noiseSeed >> 3) & 1)) << 17);
            self.noiseSeed >>= 1;
            
            return self.currentNoiseBit
        }
    }
    
    private var currentNoiseBit: Bool {
        get {
            return self.noiseSeed & 1 == 1
        }
    }
    
    public lazy var sourceNode = AVAudioSourceNode { _, _, frameCount, audioBufferList in
        let ablPointer = UnsafeMutableAudioBufferListPointer(audioBufferList)

        for frame in 0..<Int(frameCount) {
            var sampleVal = Float(0)
            var noiseBit = self.currentNoiseBit
            if self.noiseCount == self.noisePeriod {
                var nextNoiseBit = self.nextNoiseBit
                if (self.noiseEnabled) {
                    noiseBit = nextNoiseBit
                }
            }
            
            var amplitudeForSample = self.amplitude
            if (!self.amplitudeFixedMode) {
                // amplitudeForSample = calculateEnvelope
            }
            
            var toneBit = true
            if self.toneCount <= (self.tonePeriod / 2) {
                toneBit = false
            }
            
            if (!self.toneEnabled || toneBit) && (!self.noiseEnabled || noiseBit) {
                sampleVal = amplitudeForSample
            } else {
                sampleVal = 0
            }
            
            self.toneCount += 1
            if self.toneCount > self.tonePeriod {
                self.toneCount = 0
            }
            
            self.noiseCount += 1
            if self.noiseCount > self.noisePeriod {
                self.noiseCount = 0
            }
            
            //self.envelopeCount += 1 // TODO how do the double period envelopes work?
            
            for buffer in ablPointer {
                let buf: UnsafeMutableBufferPointer<Float> = UnsafeMutableBufferPointer(buffer)
                buf[frame] = sampleVal
            }
        }
        
        return noErr
    }
}

@objc
class Synth : NSObject {
    
    @objc
    public static let shared = Synth()
    
    public var volume: Float {
        set {
            audioEngine.mainMixerNode.outputVolume = newValue
        }
        get {
            audioEngine.mainMixerNode.outputVolume
        }
    }
    
    @objc public var channelA:ChannelGenerator
    @objc public var channelB:ChannelGenerator
    @objc public var channelC:ChannelGenerator
    
    @objc
    public var envelopeShape: envelopeShape = FadeDownOnceHoldLow {
        didSet {
            switch envelopeShape {
                case FadeDownOnceHoldLow:
                    self.envelope = Envelope.fadeDownOnceHoldLow
                    break
                case FadeUpOnceHoldLow:
                    self.envelope = Envelope.fadeDownOnceHoldLow
                    break
                case DescendingSawTooth:
                    self.envelope = Envelope.descendingSawTooth
                    break
                case DescendingTriangle:
                    self.envelope = Envelope.descendingTriangle
                    break
                case FadeDownOnceHoldHigh:
                    self.envelope = Envelope.fadeUpOnceHoldHigh
                    break
                case AscendingSawTooth:
                    self.envelope = Envelope.ascendingSawTooth
                    break
                case FadeUpOnceHoldHigh:
                    self.envelope = Envelope.fadeUpOnceHoldHigh
                    break
                case AscendingTriangle:
                    self.envelope = Envelope.ascendingTriangle
                    break
                default:
                    break
            }
            
            self.channelA.envelope = self.envelope
            self.channelB.envelope = self.envelope
            self.channelC.envelope = self.envelope
        }
    }
      
    private var envelope: Signal = Envelope.fadeDownOnceHoldLow
    
    @objc public var noiseFrequency: Float = 1 {
        didSet {
            self.channelA.noiseFrequency = noiseFrequency
            self.channelB.noiseFrequency = noiseFrequency
            self.channelC.noiseFrequency = noiseFrequency
        }
    }
    
    @objc public var envelopeFrequency: Float = 1 {
        didSet {
            self.channelA.envelopeFrequency = envelopeFrequency
            self.channelB.envelopeFrequency = envelopeFrequency
            self.channelC.envelopeFrequency = envelopeFrequency
        }
    }
    
    private var audioEngine: AVAudioEngine
    private let sampleRate: Double
    private let deltaTime: Float
    
    // MARK: Init
    
    override init() {
        NSLog("Initializing swift")
        audioEngine = AVAudioEngine()
        
        let mainMixer = audioEngine.mainMixerNode
        let outputNode = audioEngine.outputNode
        let format = outputNode.inputFormat(forBus: 0)
        
        sampleRate = format.sampleRate
        deltaTime = 1 / Float(sampleRate)
        
        self.channelA = ChannelGenerator(sampleRate: Int(sampleRate))
        self.channelB = ChannelGenerator(sampleRate: Int(sampleRate))
        self.channelC = ChannelGenerator(sampleRate: Int(sampleRate))
        
        let inputFormat = AVAudioFormat(commonFormat: format.commonFormat,
                                        sampleRate: format.sampleRate,
                                        channels: 3,
                                        interleaved: format.isInterleaved)
        
        super.init()
        
        let sourceNodeA = self.channelA.sourceNode
        let sourceNodeB = self.channelB.sourceNode
        let sourceNodeC = self.channelC.sourceNode
        audioEngine.attach(sourceNodeA)
        audioEngine.attach(sourceNodeB)
        audioEngine.attach(sourceNodeC)
        audioEngine.connect(sourceNodeA, to: mainMixer, format: inputFormat)
        audioEngine.connect(sourceNodeB, to: mainMixer, format: inputFormat)
        audioEngine.connect(sourceNodeC, to: mainMixer, format: inputFormat)
        audioEngine.connect(mainMixer, to: outputNode, format: nil)
        mainMixer.outputVolume = 0.3
        
        do {
            try audioEngine.start()
        } catch {
            print("Could not start engine: \(error.localizedDescription)")
        }
    }
}
