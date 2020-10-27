using System;
using System.Net.Sockets;

namespace Protocoles
{
    public static class CommandProtocol
    {
        public static CommandPackage RecieveCommand(Socket socket)
        {
            var buffer = new byte[HeaderConstants.HeaderLength];
            RecieveData(socket, HeaderConstants.HeaderLength, buffer);
            
            CommandPackage package = new CommandPackage();
            package.DecodeHeader(buffer);

            var bufferMessage = new byte[package.DataLength];
            RecieveData(socket, package.DataLength, bufferMessage);

            package.DecodeMessage(bufferMessage);
            return package;
        }

        private static void RecieveData(Socket socket, int length, byte[] buffer)
        {
            var iRecv = 0;
            while (iRecv < length)
            {
                try
                {
                    var localRecv = socket.Receive(buffer, iRecv, length - iRecv, SocketFlags.None);
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
        }

        public static void SendCommand(Socket socket, CommandPackage package)
        {
            var data = package.GetHeader();
            var sentBytes = 0;
            while (sentBytes < data.Length)
            {
                sentBytes += socket.Send(data, sentBytes, data.Length - sentBytes, SocketFlags.None);
            }
            sentBytes = 0;
            var message = package.GetMessage();
            while (sentBytes < message.Length)
            {
                sentBytes += socket.Send(message, sentBytes, message.Length - sentBytes, SocketFlags.None);
            }
        }
    }
}
