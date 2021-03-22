//  
// Copyright (c) 2021, Nathan Randle. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
namespace CPCSharp.Core {
    public enum PhysicalMemoryComponent {
        RAM,
        LowerROM,
        UpperROM
    }
    public struct MemoryReadLocation {
        public PhysicalMemoryComponent Component { get; }

        public ushort ComponentLocalAddress { get; }

        public MemoryReadLocation(PhysicalMemoryComponent component, ushort address) {
            Component = component;
            ComponentLocalAddress = address;
        }
    }
}