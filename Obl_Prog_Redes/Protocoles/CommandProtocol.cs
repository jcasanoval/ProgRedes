using System;
using System.Net.Sockets;

namespace Protocoles
{
    public static class CommandProtocol
    {
        public static byte[] RecieveCommand(Socket socket)
        {
            var buffer = new byte[HeaderConstants.HeaderLength];
            var iRecv = 0;
            while (iRecv < HeaderConstants.HeaderLength)
            {
                try
                {
                    var localRecv = socket.Receive(buffer, iRecv, HeaderConstants.HeaderLength - iRecv, SocketFlags.None);
                    if (localRecv == 0)
                    {
                        socket.Shutdown(SocketShutdown.Both);
                        socket.Close();
                    }
                    iRecv += localRecv;
                }
                catch (SocketException se)
                {
                    Console.WriteLine(se.Message);
                }
            }
            return buffer;
        }
    }
}
