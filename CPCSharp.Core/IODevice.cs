namespace CPCSharp.Core
{
    public interface IODevice {
        ushort Address { set; }
        byte Data { get; set; }
        bool ActiveAtAddress(ushort address);
    }
}