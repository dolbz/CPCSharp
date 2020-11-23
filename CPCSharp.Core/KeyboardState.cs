using System.Collections.Generic;

namespace CPCSharp.Core {
    public class CPCKey {
        public static CPCKey A = new CPCKey { line = 8, bitPos = 5 };
        public int line { get; private set; }
        public int bitPos { get; private set; }
    }

    public class KeyboardState {
        private static KeyboardState instance = null;
        private static readonly object padlock = new object();

        private byte[] keyLines = new byte[10];

        private KeyboardState()
        {
            for (int i = 0; i < keyLines.Length; i++) {
                keyLines[i] = 0xff;
            }
        }

        public static KeyboardState Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new KeyboardState();
                    }
                    return instance;
                }
            }
        }

        public byte KeyStateForLine(int lineNumber) {
            return keyLines[lineNumber];
        }

        public void KeyUp(CPCKey key) {
            var currentLine = keyLines[key.line];
            var bitToSet = 0x1 << key.bitPos;
            keyLines[key.line] = (byte)(currentLine | bitToSet);
        }

        public void KeyDown(CPCKey key) {
            var currentLine = keyLines[key.line];
            var bitToReset = ~(0x1 << key.bitPos);
            keyLines[key.line] = (byte)(currentLine & bitToReset);
        }
    }
}