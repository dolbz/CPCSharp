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

enum envelopeStyle {
    FadeDownOnceHoldLow,
    FadeUpOnceHoldLow,
    DescendingSawTooth,
    DescendingTriangle,
    FadeDownOnceHoldHigh,
    AscendingSawTooth,
    FadeUpHoldHigh,
    AscendingTriangle
};

void SetToneNative(enum channel channel, float frequency);

void SetChannelAttributesNative(enum channel channel, bool channelEnabled, bool noiseOn);

void SetAmplitudeModeNative(enum channel channel, bool isFixedMode);

void SetAmplitudeNative(enum channel channel, float amplitude);

@end

