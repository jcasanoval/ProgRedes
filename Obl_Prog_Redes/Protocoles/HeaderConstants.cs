using System;
using System.Collections.Generic;
using System.Text;

namespace Protocoles
{
    public class HeaderConstants
    {
        public static string Request = "REQ";
        public static string Response = "RES";
        public static int DirectionLength = Request.Length;
        public static int CommandLength = 2;
        public static int DataLength = 4;
        public static int HeaderLength = DirectionLength + CommandLength + DataLength;
    }
}
