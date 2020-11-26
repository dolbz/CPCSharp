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