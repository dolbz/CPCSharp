//  
// Copyright (c) 2021, Nathan Randle. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
namespace CPCSharp.Core
{
    public interface IODevice {
        ushort Address { set; }
        byte Data { get; set; }
        bool ActiveAtAddress(ushort address);
    }
}