using System;
using System.Collections.Generic;
using System.Text;

namespace Protocoles
{
    public class HeaderConstants
    {
        public static int DirectionLength = 3;
        public static int CommandLength = 2;
        public static int DataLength = 4;
        public static int HeaderLength = DirectionLength + CommandLength + DataLength;
    }
}
