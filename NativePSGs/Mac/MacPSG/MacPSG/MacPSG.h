//
//  MacPSG.h
//  MacPSG
//
//  Created by Nathan Randle on 02/02/2021.
//

#import <Foundation/Foundation.h>

@interface MacPSG : NSObject

//void SetEnvelopeA(enum channel channel, float frequency, int style);

enum channel { ChannelA, ChannelB, ChannelC };

enum envelopeShape {
    FadeDownOnceHoldLow,
    FadeUpOnceHoldLow,
    DescendingSawTooth,
    DescendingTriangle,
    FadeDownOnceHoldHigh,
    AscendingSawTooth,
    FadeUpOnceHoldHigh,
    AscendingTriangle
};

void SetToneNative(enum channel channel, float frequency);

void SetChannelAttributesNative(enum channel channel, bool channelEnabled, bool noiseOn);

void SetAmplitudeModeNative(enum channel channel, bool isFixedMode);

void SetAmplitudeNative(enum channel channel, float amplitude);

void SetEnvelopeFrequencyNative(float frequency);

void SetEnvelopeShapeNative(enum envelopeShape shape);

void SetNoiseFrequencyNative(float frequency);

@end

