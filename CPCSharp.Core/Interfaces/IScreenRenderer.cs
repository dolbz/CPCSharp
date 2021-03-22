//  
// Copyright (c) 2021, Nathan Randle. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using System.Drawing;

namespace CPCSharp.Core.Interfaces {
    public interface IScreenRenderer {
        void ResolutionChanged(Size dimensions);
        void SendPixels(Color[] pixels);
        void SendHsyncStart();
        void SendHsyncEnd();
        void SendVsyncStart();
        void SendVsyncEnd();
    }
}