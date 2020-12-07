namespace CPCSharp.Core {
    public class CPCKey {
        public static CPCKey A = new CPCKey { line = 8, bitPos = 5 };
        public static CPCKey B = new CPCKey { line = 6, bitPos = 6 };
        public static CPCKey C = new CPCKey { line = 7, bitPos = 6 };
        public static CPCKey D = new CPCKey { line = 7, bitPos = 5 };
        public static CPCKey E = new CPCKey { line = 7, bitPos = 2 };
        public static CPCKey F = new CPCKey { line = 6, bitPos = 5 };
        public static CPCKey G = new CPCKey { line = 6, bitPos = 4 };
        public static CPCKey H = new CPCKey { line = 5, bitPos = 4 };
        public static CPCKey I = new CPCKey { line = 4, bitPos = 3 };
        public static CPCKey J = new CPCKey { line = 5, bitPos = 5 };
        public static CPCKey K = new CPCKey { line = 4, bitPos = 5 };
        public static CPCKey L = new CPCKey { line = 4, bitPos = 4 };
        public static CPCKey M = new CPCKey { line = 4, bitPos = 6 };
        public static CPCKey N = new CPCKey { line = 5, bitPos = 6 };
        public static CPCKey O = new CPCKey { line = 4, bitPos = 2 };
        public static CPCKey P = new CPCKey { line = 3, bitPos = 3 };
        public static CPCKey Q = new CPCKey { line = 8, bitPos = 3 };
        public static CPCKey R = new CPCKey { line = 6, bitPos = 2 };
        public static CPCKey S = new CPCKey { line = 7, bitPos = 4 };
        public static CPCKey T = new CPCKey { line = 6, bitPos = 3 };
        public static CPCKey U = new CPCKey { line = 5, bitPos = 2 };
        public static CPCKey V = new CPCKey { line = 6, bitPos = 7 };
        public static CPCKey W = new CPCKey { line = 7, bitPos = 3 };
        public static CPCKey X = new CPCKey { line = 7, bitPos = 7 };
        public static CPCKey Y = new CPCKey { line = 5, bitPos = 3 };
        public static CPCKey Z = new CPCKey { line = 8, bitPos = 7 };
        public static CPCKey Num0 = new CPCKey { line = 4, bitPos = 0 };
        public static CPCKey Num1 = new CPCKey { line = 8, bitPos = 0 };
        public static CPCKey Num2 = new CPCKey { line = 8, bitPos = 1 };
        public static CPCKey Num3 = new CPCKey { line = 7, bitPos = 1 };
        public static CPCKey Num4 = new CPCKey { line = 7, bitPos = 0 };
        public static CPCKey Num5 = new CPCKey { line = 6, bitPos = 1 };
        public static CPCKey Num6 = new CPCKey { line = 6, bitPos = 0 };
        public static CPCKey Num7 = new CPCKey { line = 5, bitPos = 1 };
        public static CPCKey Num8 = new CPCKey { line = 5, bitPos = 0 };
        public static CPCKey Num9 = new CPCKey { line = 4, bitPos = 1 }; 
        public static CPCKey ForwardSlash = new CPCKey { line = 3, bitPos = 6 };
        public static CPCKey BackSlash = new CPCKey { line = 2, bitPos = 6 };
        public static CPCKey SemiColon = new CPCKey { line = 3, bitPos = 4 };
        public static CPCKey Colon = new CPCKey { line = 3, bitPos = 5 };
        public static CPCKey Space = new CPCKey { line = 5, bitPos = 7};
        public static CPCKey Return = new CPCKey { line = 2, bitPos = 2 };
        public static CPCKey CurUp = new CPCKey { line = 0, bitPos = 0 };
        public static CPCKey CurDown = new CPCKey { line = 0, bitPos = 2 };
        public static CPCKey CurLeft = new CPCKey { line = 1, bitPos = 0 };
        public static CPCKey CurRight = new CPCKey { line = 0, bitPos = 1 };
        public static CPCKey Shift = new CPCKey { line = 2, bitPos = 5 };
        public static CPCKey Delete = new CPCKey { line = 9, bitPos = 7 };
        public static CPCKey Comma = new CPCKey { line = 4, bitPos = 7 };

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