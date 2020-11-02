using System;
using System.Collections.Generic;
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
            try
            {
                RecieveData(socket, package.DataLength, bufferMessage);
                package.DecodeMessage(bufferMessage);
            }
            catch
            {
                return package;
            }
            
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
                        throw new Exception() ;
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

        public static void SendList(Socket socket, List<String> list, int command)
        {
            CommandPackage package;
            foreach (String s in list)
            {
                package = new CommandPackage(HeaderConstants.Response, command, s);
                SendCommand(socket, package);
                RecieveCommand(socket);
            }
            package = new CommandPackage(HeaderConstants.Response, CommandConstants.FinishSendingList);
            SendCommand(socket, package);
        }

        public static List<String> RecieveList(Socket socket)
        {
            CommandPackage package = RecieveCommand(socket);
            List<String> list = new List<String>();
            while (package.Command != CommandConstants.FinishSendingList)
            {
                list.Add(package.Data);
                CommandPackage ack = new CommandPackage(HeaderConstants.Request, CommandConstants.ACK);
                SendCommand(socket, ack);
                package = RecieveCommand(socket);
            }
            return list;
        }
    }
}
