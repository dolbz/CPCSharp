//  
// Copyright (c) 2021, Nathan Randle. All rights reserved.  
// Licensed under the MIT License. See LICENSE file in the project root for full license information.  
// 
using CPCSharp.Core.PSG;

namespace CPCSharp.Core {

    /// <summary>
    /// Direction is in respect of the CPU the PPI is attached to NOT the PPI itself
    /// </summary>
    public enum IODirection {
        Output = 0,
        Input = 1
        
    }

    /// <summary>
    /// PPI implementation doesn't accurately emulate the actual 8255 package but integrates components 
    /// with respect to how they're integrated on the CPC so the behaviour is correct for the 
    /// overall CPC emulator.
    /// 
    /// I/O address	A9	A8	Description	Read/Write status	Used Direction	Used for
    /// 0xF4xx	    0	0	Port A Data	Read/Write	        In/Out	        PSG (Sound/Keyboard/Joystick)
    /// 0xF5xx	    0	1	Port B Data	Read/Write	        In	            Vsync/Jumpers/PrinterBusy/CasIn/Exp
    /// 0xF6xx	    1	0	Port C Data	Read/Write	        Out	            KeybRow/CasOut/PSG
    /// 0xF7xx	    1	1	Control	    Write Only	        Out	            Control
    /// </summary>
    public class PPI : IODevice
    {
        private const int PortAAddressMask = 0xf400;
        private const int PortBAddressMask = 0xf500;
        private const int PortCAddressMask = 0xf600;
        private const int ControlAddressMask = 0xf700;

        private readonly CRTC _crtc;
        private byte _ioControlData = 0;
        private byte _portCLatchedOutput;

        private IODirection PortADirection => (IODirection)((_ioControlData >> 4) & 0x1);

        private readonly AY8912 _psg;

        private bool _casIn;
        public bool CassetteIn { get => _casIn; set {
            _casIn = value;
        } }

        public byte Data { 
            get {
                var maskedAddress = (Address & 0xf700);
                if (maskedAddress == PortAAddressMask && PortADirection == IODirection.Input) {
                    return _psg.Data;
                }
                else if (maskedAddress == PortBAddressMask) {
                    var casIn = (CassetteIn ? 0x1 : 0x0) << 7;
                    byte manufacturer = 0b00001110;
                    byte vsync = (byte)(_crtc.VSYNC ? 0x1 : 0x0);
                    byte refreshRate = 0b00010000;

                    var val = (byte)(casIn | refreshRate | manufacturer | vsync);
                    return val;
                }
                return 0; // Shouldn't technically hit this...
            } 
            set {
                var maskedAddress = (Address & 0xf700);
                if (maskedAddress == PortAAddressMask && PortADirection == IODirection.Output) {
                    _psg.Data = value;
                }
                else if (maskedAddress == PortCAddressMask) {
                    _portCLatchedOutput = value;
                    ApplyPortCOutput(_portCLatchedOutput);
                }
                else if (maskedAddress == ControlAddressMask) {
                    if ((value & 0x80) == 0) { // Bit 7 is zero - Bit Set Reset Mode
                        var bitSetResetData = value & 0xf;
                        var newValue = bitSetResetData & 0x1;
                        var bitToSet = bitSetResetData >> 1;
                        var bitValueInPosition = (byte)(1 << bitToSet);
                        if (newValue == 0) {
                            _portCLatchedOutput &= (byte)~bitValueInPosition;
                        } else {
                            _portCLatchedOutput |= bitValueInPosition;
                        }
                        ApplyPortCOutput(_portCLatchedOutput);
                    } 
                    else 
                    {
                        _ioControlData = value;
                    }
                }
            } 
        }

        public bool TapeMotorOn { get; private set; }

        private ushort _address;
        public ushort Address 
        { 
            private get => _address; 
            set 
            {
                _address = value;
                // TODO if address is a port configured as output, set data value
            }
        }

        public PPI(CRTC crtc, INativePSG psg) {
            _crtc = crtc;
            _psg = new AY8912(psg);
        }

        public bool ActiveAtAddress(ushort address)
        {
            return (address & 0xfc00) == 0xf400;
        }
        
        private void ApplyPortCOutput(byte output) {
            _psg.KeyboardLine = (byte)(output & 0x0f);
            _psg.BDIR = (output >> 7) == 0x1;
            _psg.BC1 = ((output >> 6) & 0x1) == 0x1;
            TapeMotorOn = ((output >> 4) & 0x1) == 0x1;
            _psg.InputTransitionComplete();
        }
    }
}