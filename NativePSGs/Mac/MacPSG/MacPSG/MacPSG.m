//
//  MacPSG.m
//  MacPSG
//
//  Created by Nathan Randle on 02/02/2021.
//

#import "MacPSG.h"
#import "MacPSG-Swift.h"

@implementation MacPSG

@end

ChannelGenerator* GetChannelGeneratorForChannel(enum channel channel) {
    switch (channel) {
        case ChannelA:
            return Synth.shared.channelA;
            break;
        case ChannelB:
            return Synth.shared.channelB;
            break;
        case ChannelC:
            return Synth.shared.channelC;
            break;
    }
}

void SetToneNative(enum channel channel, float frequency) {
    GetChannelGeneratorForChannel(channel).frequency = frequency;
}

void SetChannelAttributesNative(enum channel channel, bool channelEnabled, bool noiseOn) {
    ChannelGenerator* cg = GetChannelGeneratorForChannel(channel);
    cg.toneEnabled = channelEnabled;
    cg.noiseEnabled = noiseOn;
}

void SetAmplitudeModeNative(enum channel channel, bool isFixedMode) {
    GetChannelGeneratorForChannel(channel).amplitudeFixedMode = isFixedMode;
}

void SetAmplitudeNative(enum channel channel, float amplitude) {
    GetChannelGeneratorForChannel(channel).amplitude = amplitude;
}

void SetEnvelopeFrequencyNative(float frequency) {
    Synth.shared.envelopeFrequency = frequency;
}

void SetEnvelopeShapeNative(enum envelopeShape shape) {
    Synth.shared.envelopeShape = shape;
}

void SetNoiseFrequencyNative(float frequency) {
    Synth.shared.noiseFrequency = frequency;
}
