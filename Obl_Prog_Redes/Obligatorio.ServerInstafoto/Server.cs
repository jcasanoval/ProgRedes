using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using RabbitMQ.Client;
using Common.Logs;

namespace Obligatorio.ServerInstafoto
{
    public class Server
    {
        private const String separador = "---------------------------------------------------------------";
        private List<User> users = new List<User>();
        public List<Connection> connections = new List<Connection>();
        private static Server serverInstance;
        public static int pictureCount = 0;
        private const string ExchangeName = "logExchange";
        static object registerLocker;

        public static bool keepRunning = true;

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
            while (keepRunning)
            {

                var client = server.Accept();
                if (keepRunning)
                {
                    Connection newConnection = new Connection();
                    connections.Add(newConnection);
                    newConnection.Socket = client;
                    newConnection.StartConnection(client);
                }
                else
                {
                    Console.WriteLine("Server is shutting down");
                }

            }

        }

        public void LogAction(ILog log)
        {
            var connectionFactory = new ConnectionFactory { HostName = "localhost" };
            using IConnection connection = connectionFactory.CreateConnection();
            using IModel channel = connection.CreateModel();
            string json = LogHandler.Serialize(log);
            ExchangeDeclare(channel);
            PublishMessage(channel, json);
        }
        private static void ExchangeDeclare(IModel channel)
        {
            channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct);
        }
        private static void PublishMessage(IModel channel, string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(ExchangeName, String.Empty, null, body);
        }
        private static void ServerMenu()
        {
            while (keepRunning)
            {
                Console.WriteLine(separador);
                Console.WriteLine("Ingrese exit para apagar el servidor");
                var input = Console.ReadLine();
                switch (input)
                {
                    case "exit":
                        foreach (Connection connection in Server.GetInstance().connections)
                        {
                            connection.Socket.Shutdown(SocketShutdown.Both);
                            connection.Socket.Close();

                        }
                        var trapSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        trapSocket.Connect("127.0.0.1", 6000);

                        keepRunning = false;
                        break;
                }
            }
        }

        public bool DeleteUser(string username)
        {
            foreach (User user in users)
            {
                if (user.Name == username)
                {
                    users.Remove(user);
                    return true;
                }
            }
            return false;
        }

        public string ModUser(string name, string newName, string newPassword)
        {
            var user = users.Find(x => x.Name == name);
            if (user == null)
            {
                return "No se encuentra el usuario a modificar";
            }
            if (newName.Contains("%"))
            {
                return "El nombre de usuario no puede contener %";
            }
            else
            {
                if (users.Exists(x => x.Name == newName))
                {
                    return "Ese nombre de usuario ya esta en uso";
                }
            }
            if (newPassword.Contains("%"))
            {
                return "La contraseña no puede contener %";
            }
            if (newName.Trim() != "")
                user.Name = newName;
            if (newPassword != "")
                user.Password = newPassword;
            return "Usuario modificado con exito";
        }

        public User RegisterUser(string username, string password)
        {
            User newUser = new User();
            newUser.Name = username;
            newUser.Password = password;
            //lock (registerLocker)
            //{
            if (users.Exists(x => x.Name == username))
            {
                throw new Exception();
            }

            users.Add(newUser);
            //}
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
