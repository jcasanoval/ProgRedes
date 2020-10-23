using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Obligatorio.ServerClient
{
    public class Server
    {
        private List<User> users = new List<User>();
        private List<Connection> connections = new List<Connection>();
        private static Server serverInstance;

        public static Server GetInstance()
        {
            if (serverInstance == null)
            {
                serverInstance = new Server();
            }
            return serverInstance;
        }

        public void SearchConnections()
        {
            Console.WriteLine("Esperando a nuevos clientes...");
            var server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var localEp = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6000);
            server.Bind(localEp);
            server.Listen(100);
            while (true)
            {
                var client = server.Accept();
                Connection newConnection = new Connection();
                connections.Add(newConnection);
                newConnection.StartConnection(client);
            }
        }

        public void RegisterUser(string username, string password)
        {
            User newUser = new User();
            newUser.Name = username;
            newUser.Password = password;
            if (users.Contains(newUser))
            {
                //throw new UserAlreadyExistsException();
            }
            else
            {
                users.Add(newUser);
            }
        }
    }
}
