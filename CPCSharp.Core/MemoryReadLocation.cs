namespace CPCSharp.Core {
    internal enum PhysicalMemoryComponent {
        RAM,
        LowerROM,
        UpperROM
    }
    internal struct MemoryReadLocation {
        public PhysicalMemoryComponent Component { get; }

        public ushort ComponentLocalAddress { get; }

        public MemoryReadLocation(PhysicalMemoryComponent component, ushort address) {
            Component = component;
            ComponentLocalAddress = address;
        }
    }
}