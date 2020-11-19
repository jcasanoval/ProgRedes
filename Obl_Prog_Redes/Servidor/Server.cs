using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using RabbitMQ.Client;

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
                        Server.GetInstance().ABM();
                        break;
                    case "3":
                        Server.GetInstance().ListPhotos();
                        break;
                    case "4":
                        Server.GetInstance().UploadPicture();
                        break;
                    case "5":
                        Server.GetInstance().CommentPhoto();
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

        private void ABM()
        {
            Console.WriteLine(separador);
            Console.WriteLine("MENU ABM \n 1-Alta cliente \n 2-Baja cliente \n 3-Modificacion cliente");
            var input = Console.ReadLine();
            switch (input)
            {
                case "1":
                    AltaCliente();
                    break;
                case "2":
                    BajaCliente();
                    break;
                case "3":
                    ModCliente();
                    break;
                default:
                    Console.WriteLine("Opcion no valida");
                    break;
            }
        }

        private void AltaCliente()
        {
            Console.WriteLine(separador);
            Console.WriteLine("Ingrese nombre de usuario");
            bool validInput = false;
            string username = "";
            while (!validInput)
            {
                username = Console.ReadLine();
                if (username.Contains("%") || username.Trim().Length == 0)
                {
                    Console.WriteLine("El nombre de usuario no puede ser vacio ni contener %");
                }
                else
                {
                    validInput = true;
                }
            }
            validInput = false;
            Console.WriteLine("Ingrese contraseña");
            string password = "";
            while (!validInput)
            {
                password = Console.ReadLine();
                if (password.Contains("%") || password.Trim().Length == 0)
                {
                    Console.WriteLine("La contraseña no puede ser vacia ni contener %");
                }
                else
                {
                    validInput = true;
                }
            }
            try
            {
                RegisterUser(username, password);
            }
            catch
            {
                Console.WriteLine("El nombre de usuario ya esta en uso");
            }
        }

        private void BajaCliente()
        {
            Console.WriteLine(separador);
            Console.WriteLine("Ingrese el usuario que desea eliminar");
            var input = Console.ReadLine();
            foreach(User user in users)
            {
                if (user.Name == input)
                {
                    users.Remove(user);
                    return;
                }
            }
            Console.WriteLine("No se encontro el usuario indicado");
        }

        private void ModCliente()
        {
            Console.WriteLine(separador);
            Console.WriteLine("Ingrese el usuario que desea modificar");
            var user = users.Find(x => x.Name == Console.ReadLine());
            if (user == null)
            {
                Console.WriteLine("No se encuentra ese usuario en el sistema");
                return;
            }

            Console.WriteLine("Ingrese el nuevo nombre de usuario");
            bool validInput = false;
            string username = "";
            while (!validInput)
            {
                username = Console.ReadLine();
                if (username.Contains("%") || username.Trim().Length == 0)
                {
                    Console.WriteLine("El nombre de usuario no puede ser vacio ni contener %");
                }
                else
                {
                    if (users.Exists(x => x.Name == username))
                    {
                        Console.WriteLine("Ese nombre de usuario ya esta en uso");
                    }
                    else
                    {
                        validInput = true;
                    }
                }
            }
            validInput = false;
            Console.WriteLine("Ingrese la nueva contraseña");
            string password = "";
            while (!validInput)
            {
                password = Console.ReadLine();
                if (password.Contains("%") || password.Trim().Length == 0)
                {
                    Console.WriteLine("La contraseña no puede ser vacia ni contener %");
                }
                else
                {
                    validInput = true;
                }
            }
            user.Name = username;
            user.Password = password;
        }

        private void ListPhotos()
        {
            Console.WriteLine(separador);
            Console.WriteLine("Ingrese el usuario");
            var user = users.Find(x => x.Name == Console.ReadLine());
            if (user == null)
            {
                Console.WriteLine("No se encuentra ese usuario en el sistema");
                return;
            }
            foreach (string photo in user.PictureList())
            {
                Console.WriteLine(photo);
            }
        }

        private void UploadPicture()
        {

        }

        private void CommentPhoto()
        {
            Console.WriteLine(separador);
            Console.WriteLine("Ingrese el usuario al cual pertenece la foto");
            var user = users.Find(x => x.Name == Console.ReadLine());
            if (user == null)
            {
                Console.WriteLine("No se encuentra ese usuario en el sistema");
                return;
            }
            Console.WriteLine("Ingrese la foto que desea comentar");
            var photo = user.Photos.Find(x => x.Name == Console.ReadLine());
            if (photo == null)
            {
                Console.WriteLine("El usuario no cuenta con una foto con ese nombre");
                return;
            }
            Console.WriteLine("Ingrese el usuario que realiza el comentario");
            var input = Console.ReadLine();
            var commenter = users.Find(x => x.Name == input);
            if (commenter == null)
            {
                Console.WriteLine("No se encuentra ese usuario en el sistema");
                return;
            }
            Comment comment = new Comment();
            comment.User = commenter;
            Console.WriteLine("Ingrese el comentario");
            comment.Text = Console.ReadLine();
            photo.Comments.Add(comment);
        }

        public User RegisterUser(string username, string password)
        {
            User newUser = new User();
            newUser.Name = username;
            newUser.Password = password;
            if (users.Exists(x => x.Name == username))
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

        public void Log(string log)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: "logs", type: ExchangeType.Fanout); //Exchange "global"  que recibe los mensajes

                var message = log;
                var body = Encoding.UTF8.GetBytes(message);
                channel.BasicPublish(exchange: "logs",
                    routingKey: "",
                    basicProperties: null,
                    body: body);
                Console.WriteLine(" [x] Sent {0}", message);
            }
        }
    }
}
