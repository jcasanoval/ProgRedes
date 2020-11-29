using Common.Logs;
using System;

namespace Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            ILog log = new Warning();
            log.User = "yo";
            log.Message = "hola";
            string test = LogHandler.Serialize(log);
            Console.WriteLine(test);
            ILog log2 = LogHandler.Deserialize(test);
            if (log2 is Error)
                Console.WriteLine("Error");
            else
                Console.WriteLine("NOPE");
        }
    }
}
