using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Obligatorio.ServerClient
{
    public class Connection
    {
        public User User { get; set; }
        static int _clientNumber;


        public void StartConnection(Socket client)
        {
            var clientHandler = new Thread(() => HandleClient(client, this));
            clientHandler.Start();
        }
        public static void HandleClient(Socket client, Connection connection)
        {
            var id = Interlocked.Add(ref _clientNumber, 1);
            var connected = true;
            Console.WriteLine("Conectado el cliente " + id);
            SendMainMenu(client);
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

        public static void SendMainMenu(Socket client)
        {
            var message = "Bienvenido al menu principal \n Login: iniciar sesion \n SignIn: registrarse \n Exit";
            var data = Encoding.UTF8.GetBytes(message);
            var dataSent = client.Send(data);
            Console.WriteLine("bytes sent {0}", dataSent);
        }
    }
}
