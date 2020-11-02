using System;
using System.Collections.Generic;
using System.Text;

namespace Common.FileProtocol.Protocol
{
    public class Specification
    {
        public const int FixedFileNameLength = 4;
        public const int FixedFileSizeLength = 8;
        public const int MaxPacketSize = 32768; // 32KB
    }
}
