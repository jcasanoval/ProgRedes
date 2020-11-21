using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Obligatorio.ServerInstafoto
{
    public class ServerApp
    {
        static int _clientNumber;

        static void Main(string[] args)
        {
            Server.GetInstance().SearchConnections();
        }

        private static void HandleClient(Socket client)
        {
            var id = Interlocked.Add(ref _clientNumber, 1);
            var connected = true;
            Console.WriteLine("Conectado el cliente " + id);
            while (connected)
            {
                try
                {
                    var data = new byte[256];
                    var i = client.Receive(data);
                    if (i == 0)
                    {
                        Console.WriteLine("El cliente {0} cerro la conexion", id);
                        client.Shutdown(SocketShutdown.Both);
                        client.Close();
                        connected = false;
                    }
                    else
                    {
                        Console.WriteLine("Recibi {0} bytes ", i);
                        Console.WriteLine(Encoding.UTF8.GetString(data).TrimEnd());
                    }

                }
                catch (SocketException ex)
                {
                    Console.WriteLine("El cli6nte " + id + " cerró la conexión: " + ex.Message + " ErrorCode" +
                    ex.ErrorCode);
                    connected = false;
                    client.Shutdown(SocketShutdown.Both);
                    client.Close();
                }
            }
        }
    }
}

