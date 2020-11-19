namespace CPCSharp.Core {
    public class PPI : IODevice
    {
        public byte Data { 
            get {
                if ((Address & 0xf500) == 0xf500) {
                    return 0b00001110;
                }
                return 0;
            } 
            set {} 
        }

        public ushort Address { private get; set; }

        public bool ActiveAtAddress(ushort address)
        {
            // Active when 
            return (address & 0xfc00) == 0xf400;
        }
    }
}