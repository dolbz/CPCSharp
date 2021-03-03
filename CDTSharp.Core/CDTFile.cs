using System.Linq;
using System;
using System.Collections.Generic;
using CDTSharp.Core.Blocks;
using System.Collections;

namespace CDTSharp.Core
{
    public class CDTFile
    {
        public int MajorVersion { get; private set; }
        public int MinorVersion { get; private set; }

        public IBlock[] Blocks { get; private set; }

        public CDTFile(int majorVersion, int minorVersion, IEnumerable<IBlock> blocks) {
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
            Blocks = blocks.ToArray();
        }
    }
}
