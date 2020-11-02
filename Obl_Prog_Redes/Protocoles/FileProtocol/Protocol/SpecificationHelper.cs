using System;
using System.Collections.Generic;
using System.Text;

namespace Common.FileProtocol.Protocol
{
    public class SpecificationHelper
    {
        public static long GetParts(long fileSize)
        {
            var parts = fileSize / Specification.MaxPacketSize;
            return parts * Specification.MaxPacketSize == fileSize ? parts : parts + 1;
        }
    }
}
