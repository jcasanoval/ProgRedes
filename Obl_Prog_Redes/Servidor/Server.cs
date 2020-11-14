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
        private const String separador = "---------------------------------------------------------------";
        private List<User> users = new List<User>();
        private List<Connection> connections = new List<Connection>();
        private static Server serverInstance;
        public static int pictureCount = 0;

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
            var serverMenu = new Thread(() => ServerMenu());
            serverMenu.Start();
            while (true)
            {
                var client = server.Accept();
                Connection newConnection = new Connection();
                connections.Add(newConnection);
                newConnection.StartConnection(client);
            }
        }

        private static void ServerMenu()
        {
            bool keepRunning = true;
            while (keepRunning)
            {
                Console.WriteLine(separador);
                Console.WriteLine("MENU DEL SERVIDOR \n 1-Mostrar clientes conectados \n 2-ABM cliente \n 3-Listar fotos de un usuario \n 4-Subir foto \n 5-Comentar foto \n 6-Apagar el servidor");
                var input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                        Server.GetInstance().PrintConnections();
                        break;
                    case "2":
                        break;
                    case "3":
                        break;
                    case "4":
                        break;
                    case "5":
                        break;
                    case "6":
                        break;
                    default:
                        Console.WriteLine("Opcion no valida");
                        break;
                }
            }
        }

        private void PrintConnections()
        {
            foreach (Connection connection in connections)
            {
                if (connection.User != null)
                {
                    Console.WriteLine(connection.User.Name);
                }
            }
        }

        public User RegisterUser(string username, string password)
        {
            User newUser = new User();
            newUser.Name = username;
            newUser.Password = password;
            if (users.Contains(newUser))
            {
                throw new Exception();
            }
            users.Add(newUser);
            return newUser;
        }

        public User Login(String username, String password)
        {
            User user = users.Find(x => x.Name == username && x.Password == password);
            if (user == null)
            {
                throw new Exception();
            }
            return user;
        }

        public List<string> UserListStrings()
        {
            List<string> list = new List<string>();
            foreach (User user in users)
            {
                list.Add(user.Name);
            }
            return list;
        }

        public User GetUser(string name)
        {
            User res = null;
            foreach (User user in users)
            {
                if (user.Name == name)
                {
                    res = user;
                }
            }
            return res;
        }
    }
}
