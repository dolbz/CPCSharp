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
class Synth : NSObject {
    
    // MARK: Properties
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
    
    public var frequencyARampValue: Float = 0
    public var frequencyBRampValue: Float = 0
    public var frequencyCRampValue: Float = 0
    
    @objc public var channelAEnabled: Bool = false
    @objc public var channelBEnabled: Bool = false
    @objc public var channelCEnabled: Bool = false
    
    @objc public var amplitudeA: Float = 0
    @objc public var amplitudeB: Float = 0
    @objc public var amplitudeC: Float = 0
    
    @objc
    public var frequencyA: Float = 0 {
        didSet {
            if oldValue != 0 {
                frequencyARampValue = frequencyA - oldValue
            } else {
                frequencyARampValue = 0
            }
        }
    }
    
    @objc
    public var frequencyB: Float = 0 {
        didSet {
            if oldValue != 0 {
                frequencyBRampValue = frequencyB - oldValue
            } else {
                frequencyBRampValue = 0
            }
        }
    }
    
    @objc
    public var frequencyC: Float = 0 {
        didSet {
            if oldValue != 0 {
                frequencyCRampValue = frequencyC - oldValue
            } else {
                frequencyCRampValue = 0
            }
        }
    }

    private var audioEngine: AVAudioEngine
    
    private lazy var sourceNodeA = AVAudioSourceNode { _, _, frameCount, audioBufferList in
        let ablPointer = UnsafeMutableAudioBufferListPointer(audioBufferList)
                
        let localRampValue = self.frequencyARampValue
        let localFrequency = self.frequencyA - localRampValue
        
        let period = 1 / localFrequency

        for frame in 0..<Int(frameCount) {
            var sampleVal = Float(0)
            if (self.channelAEnabled) {
                let percentComplete = self.timeA / period
                sampleVal = self.signal(localFrequency + localRampValue * percentComplete, self.timeA)
                sampleVal *= self.amplitudeA
                self.timeA += self.deltaTime
                self.timeA = fmod(self.timeA, period)
            }
            
            for buffer in ablPointer {
                let buf: UnsafeMutableBufferPointer<Float> = UnsafeMutableBufferPointer(buffer)
                buf[frame] = sampleVal
            }
        }
        
        self.frequencyARampValue = 0
        
        return noErr
    }
    
    private lazy var sourceNodeB = AVAudioSourceNode { _, _, frameCount, audioBufferList in
        let ablPointer = UnsafeMutableAudioBufferListPointer(audioBufferList)
                
        let localRampValue = self.frequencyBRampValue
        let localFrequency = self.frequencyB - localRampValue
        
        let period = 1 / localFrequency

        for frame in 0..<Int(frameCount) {
            var sampleVal = Float(0)
            if (self.channelBEnabled) {
                let percentComplete = self.timeB / period
                sampleVal = self.signal(localFrequency + localRampValue * percentComplete, self.timeB)
                sampleVal *= self.amplitudeB
                self.timeB += self.deltaTime
                self.timeB = fmod(self.timeB, period)
            }
            
            for buffer in ablPointer {
                let buf: UnsafeMutableBufferPointer<Float> = UnsafeMutableBufferPointer(buffer)
                buf[frame] = sampleVal
            }
        }
        
        self.frequencyBRampValue = 0
        
        return noErr
    }
    
    private lazy var sourceNodeC = AVAudioSourceNode { _, _, frameCount, audioBufferList in
        let ablPointer = UnsafeMutableAudioBufferListPointer(audioBufferList)
                
        let localRampValue = self.frequencyCRampValue
        let localFrequency = self.frequencyC - localRampValue
        
        let period = 1 / localFrequency

        for frame in 0..<Int(frameCount) {
            var sampleVal = Float(0)
            if (self.channelCEnabled) {
                let percentComplete = self.timeC / period
                sampleVal = self.signal(localFrequency + localRampValue * percentComplete, self.timeC)
                sampleVal *= self.amplitudeC
                self.timeC += self.deltaTime
                self.timeC = fmod(self.timeC, period)
            }
            
            for buffer in ablPointer {
                let buf: UnsafeMutableBufferPointer<Float> = UnsafeMutableBufferPointer(buffer)
                buf[frame] = sampleVal
            }
        }
        
        self.frequencyCRampValue = 0
        
        return noErr
    }

    
    private var timeA: Float = 0
    private var timeB: Float = 0
    private var timeC: Float = 0
    private let sampleRate: Double
    private let deltaTime: Float
    
    private var signal: Signal
    
    
    // MARK: Init
    
    
    
    init(signal: @escaping Signal = Oscillator.square) {
        NSLog("Initializing swift")
        audioEngine = AVAudioEngine()
        
        let mainMixer = audioEngine.mainMixerNode
        let outputNode = audioEngine.outputNode
        let format = outputNode.inputFormat(forBus: 0)
        
        sampleRate = format.sampleRate
        deltaTime = 1 / Float(sampleRate)
        
        self.signal = signal
        
        let inputFormat = AVAudioFormat(commonFormat: format.commonFormat,
                                        sampleRate: format.sampleRate,
                                        channels: 3,
                                        interleaved: format.isInterleaved)
        
        super.init()
        
        audioEngine.attach(sourceNodeA)
        audioEngine.attach(sourceNodeB)
        audioEngine.attach(sourceNodeC)
        audioEngine.connect(sourceNodeA, to: mainMixer, format: inputFormat)
        audioEngine.connect(sourceNodeB, to: mainMixer, format: inputFormat)
        audioEngine.connect(sourceNodeC, to: mainMixer, format: inputFormat)
        audioEngine.connect(mainMixer, to: outputNode, format: nil)
        mainMixer.outputVolume = 1.0
        
        do {
            try audioEngine.start()
        } catch {
            print("Could not start engine: \(error.localizedDescription)")
        }
        
    }
    
    @objc
    public func DoNothing() {
        
    }
    
    // MARK: Public Functions
    @objc
    public func setWaveformTo(_ signal: @escaping Signal) {
        self.signal = signal
    }
    
}


//import Foundation
//import AVFoundation
//
//@objc
//public class SwiftTest : NSObject {
//    @objc
//    public class func TestAudio(frequency: Int) {
//        NSLog("Early hello")
//        //DispatchQueue.global(qos: .userInteractive).async {
//
//
//        NSLog("Hello From Swift")
//
//        let frequency = Float(frequency)
//        let amplitude = Float(0.5)
//        let duration = Float(5.0)
//
//
//        let twoPi = 2 * Float.pi
//
//        let sine = { (phase: Float) -> Float in
//            return sin(phase)
//        }
//
//        let whiteNoise = { (phase: Float) -> Float in
//            return ((Float(arc4random_uniform(UINT32_MAX)) / Float(UINT32_MAX)) * 2 - 1)
//        }
//
//        let sawtoothUp = { (phase: Float) -> Float in
//            return 1.0 - 2.0 * (phase * (1.0 / twoPi))
//        }
//
//        let sawtoothDown = { (phase: Float) -> Float in
//            return (2.0 * (phase * (1.0 / twoPi))) - 1.0
//        }
//
//        let square = { (phase: Float) -> Float in
//            if phase <= Float.pi {
//                return 1.0
//            } else {
//                return -1.0
//            }
//        }
//
//        let triangle = { (phase: Float) -> Float in
//            var value = (2.0 * (phase * (1.0 / twoPi))) - 1.0
//            if value < 0.0 {
//                value = -value
//            }
//            return 2.0 * (value - 0.5)
//        }
//
//        var signal: (Float) -> Float
//
//        signal = square
//
//        let engine = AVAudioEngine()
//        let mainMixer = engine.mainMixerNode
//        let output = engine.outputNode
//        let outputFormat = output.inputFormat(forBus: 0)
//        let sampleRate = Float(outputFormat.sampleRate)
//        // Use output format for input but reduce channel count to 1
//        let inputFormat = AVAudioFormat(commonFormat: outputFormat.commonFormat,
//                                        sampleRate: outputFormat.sampleRate,
//                                        channels: 1,
//                                        interleaved: outputFormat.isInterleaved)
//
//        var currentPhase: Float = 0
//        // The interval by which we advance the phase each frame.
//        let phaseIncrement = (twoPi / sampleRate) * frequency
//
//        let srcNode = AVAudioSourceNode { _, _, frameCount, audioBufferList -> OSStatus in
//            let ablPointer = UnsafeMutableAudioBufferListPointer(audioBufferList)
//            for frame in 0..<Int(frameCount) {
//                // Get signal value for this frame at time.
//                let value = signal(currentPhase) * amplitude
//                // Advance the phase for the next frame.
//                currentPhase += phaseIncrement
//                if currentPhase >= twoPi {
//                    currentPhase -= twoPi
//                }
//                if currentPhase < 0.0 {
//                    currentPhase += twoPi
//                }
//                // Set the same value on all channels (due to the inputFormat we have only 1 channel though).
//                for buffer in ablPointer {
//                    let buf: UnsafeMutableBufferPointer<Float> = UnsafeMutableBufferPointer(buffer)
//                    buf[frame] = value
//                }
//            }
//            return noErr
//        }
//
//        engine.attach(srcNode)
//
//        engine.connect(srcNode, to: mainMixer, format: inputFormat)
//        engine.connect(mainMixer, to: output, format: outputFormat)
//        mainMixer.outputVolume = 0.5
//
//        do {
//            try engine.start()
//            //CFRunLoopRunInMode(.defaultMode, CFTimeInterval(duration), false)
//           // engine.stop()
//        } catch {
//            print("Could not start engine: \(error)")
//        }
//        //}
//    }
//}

