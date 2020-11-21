using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Obligatorio.LogsServer
{
    public interface ILog
    {
        public string Message { get; set; }
    }
}
