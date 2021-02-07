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

void SetToneNative(enum channel channel, float frequency) {
    
    switch (channel) {
        case ChannelA:
            Synth.shared.frequencyA = frequency;
            break;
        case ChannelB:
            Synth.shared.frequencyB = frequency;
            break;
        case ChannelC:
            Synth.shared.frequencyC = frequency;
            break;
    }
}

void SetChannelAttributesNative(enum channel channel, bool channelEnabled, bool noiseOn) {
    switch (channel) {
        case ChannelA:
            Synth.shared.channelAEnabled = channelEnabled;
            break;
        case ChannelB:
            Synth.shared.channelBEnabled = channelEnabled;
            break;
        case ChannelC:
            Synth.shared.channelCEnabled = channelEnabled;
            break;
    }
}

void SetAmplitudeModeNative(enum channel channel, bool isFixedMode) {
    
}

void SetAmplitudeNative(enum channel channel, float amplitude) {
    switch (channel) {
        case ChannelA:
            Synth.shared.amplitudeA = amplitude;
            break;
        case ChannelB:
            Synth.shared.amplitudeB = amplitude;
            break;
        case ChannelC:
            Synth.shared.amplitudeC = amplitude;
            break;
    }
}
