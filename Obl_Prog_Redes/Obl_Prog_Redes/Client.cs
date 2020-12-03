using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Common.FileProtocol.NetworkUtils;
using Common.CommandProtocol;
using System.Collections.Generic;
using Common.FileProtocol.FileManager;
using Common.FileProtocol.Protocol;

namespace Client
{
    class Client
    {
        private const String separador = "---------------------------------------------------------------";
        static void Main(string[] args)
        {
            var isRunning = true;
            var clientEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 0);
            var serverEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6000);
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(clientEndpoint);
            socket.Connect(serverEndpoint);
            Console.WriteLine("conectado");
            try
            {
                while (isRunning)
                {
                    MainMenu(ref isRunning, socket);
                }
               
                Console.WriteLine("se cerro la conexion");
            }
            catch (SocketException ex)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                Console.WriteLine("se cerro la conexion");
            }
        }

        private static void MainMenu(ref bool keepRunning, Socket socket)
        {
            CommandPackage package = new CommandPackage(HeaderConstants.Request, CommandConstants.RequestLoggedUser);
            CommandProtocol.SendCommand(socket, package);
            CommandPackage response = CommandProtocol.RecieveCommand(socket);
            if (response.Data == MessageConstants.NoUserFound)
            {
                LoginMainMenu(ref keepRunning, socket);
            }
            else
            {
                LoggedMainMenu(ref keepRunning, socket, response.Data);
            }
        }

        private static void LoginMainMenu(ref bool keepRunning, Socket socket)
        {
            Console.WriteLine(separador);
            Console.WriteLine("Bienvenido al menu principal, ingresa a tu cuenta o registrate \n 1-Login \n 2-Register \n 3-Exit");
            var input = Console.ReadLine();
            switch (input)
            {
                case "1":
                    LoginMenu(socket);
                    break;
                case "2":
                    RegisterMenu(socket);
                    break;
                case "3":
                    CommandPackage package = new CommandPackage(HeaderConstants.Request, CommandConstants.Exit);
                    CommandProtocol.SendCommand(socket, package);
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                    keepRunning = false;
                    break;
                default:
                    Console.WriteLine("Opcion no valida");
                    break;
            }
        }

        private static void LoginMenu(Socket socket)
        {
            Console.WriteLine(separador);
            Console.WriteLine("Ingrese nombre de usuario");
            var username = Console.ReadLine();
            Console.WriteLine("Ingrese contraseña");
            var password = Console.ReadLine();
            CommandPackage package = new CommandPackage(HeaderConstants.Request, CommandConstants.Login, username + "%" + password);
            CommandProtocol.SendCommand(socket, package);
            CommandPackage response = CommandProtocol.RecieveCommand(socket);
            if (response.Data == MessageConstants.FailedLogin)
            {
                Console.WriteLine("Usuario y/o contraseña incorrectos");
            }
        }

        private static void RegisterMenu(Socket socket)
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
                    Console.WriteLine("Su nombre de usuario no puede ser vacio ni contener %");
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
                    Console.WriteLine("Su contraseña no puede ser vacia ni contener %");
                }
                else
                {
                    validInput = true;
                }
            }
            CommandPackage package = new CommandPackage(HeaderConstants.Request, CommandConstants.Register, username + "%" + password);
            CommandProtocol.SendCommand(socket, package);
            CommandPackage response = CommandProtocol.RecieveCommand(socket);
            if (response.Data == MessageConstants.FailedRegister)
            {
                Console.WriteLine("Ese nombre de usuario ya esta en uso");
            }
        }

        private static void LoggedMainMenu(ref bool keepRunning, Socket socket, String username)
        {
            Console.WriteLine(separador);
            Console.WriteLine("Bienvenido {0}!", username);
            Console.WriteLine("\n 1-Subir Foto \n 2-Listado usuarios sistema \n 3-Listado fotos usuario \n 4-Ver comentarios de mis fotos \n 5-Agregar comentario \n 6-Salir");
            var input = Console.ReadLine();
            switch (input)
            {
                case "1":
                    UploadPicture(socket);
                    break;
                case "2":
                    ListUsers(socket);
                    break;
                case "3":
                    ListPhotos(socket);
                    break;
                case "4":
                    ListComments(socket);
                    break;
                case "5":
                    AddComment(socket);
                    break;
                case "6":
                    CommandPackage package = new CommandPackage(HeaderConstants.Request, CommandConstants.Logout);
                    CommandProtocol.SendCommand(socket, package);
                    break;
                default:
                    Console.WriteLine("Opcion no valida");
                    break;
            }
        }

        private static void UploadPicture(Socket socket)
        {
            Console.WriteLine("Por favor inserte el path de la imagen que desea subir");
            string path = Console.ReadLine();
            while(path.Equals(string.Empty) || !FileHandler.FileExists(path))
            {
                Console.WriteLine("path invalido");
                path = Console.ReadLine();
            }

            Console.WriteLine("Ingrese el nombre que desea darle a la foto");
            string name = Console.ReadLine();
            CommandPackage package = new CommandPackage(HeaderConstants.Request, CommandConstants.SendPicture, name);
            CommandProtocol.SendCommand(socket, package);

            FileProtocol fileProtocol = new FileProtocol(socket);
            fileProtocol.SendFile(path);
            Console.WriteLine("Su foto fue subida con exito");
        }

        private static void ListUsers(Socket socket)
        {
            CommandPackage package = new CommandPackage(HeaderConstants.Request, CommandConstants.UserList);
            CommandProtocol.SendCommand(socket, package);
            List<string> list = CommandProtocol.RecieveList(socket);
            foreach(string user in list)
            {
                Console.WriteLine(user);
            }
        }

        private static void ListPhotos(Socket socket)
        {
            Console.WriteLine("Inserte el usuario del cual desea ver las fotos, si desea ver las suyas puede dejar en blanco");
            var user = Console.ReadLine();
            CommandPackage package = new CommandPackage(HeaderConstants.Request, CommandConstants.PictureList, user);
            CommandProtocol.SendCommand(socket, package);

            CommandPackage response = CommandProtocol.RecieveCommand(socket);
            if (response.Command == CommandConstants.PictureList)
            {
                List<string> pictures = CommandProtocol.RecieveList(socket);

                if (pictures.Count != 0)
                {
                    if (user == "")
                    {
                        Console.WriteLine("Sus fotos son:");
                    }
                    else
                    {
                        Console.WriteLine("Las fotos de " + user + " son:");
                    }
                    
                    foreach (string picture in pictures)
                    {
                        Console.WriteLine(picture);
                    }
                }
                else
                {
                    if (user == "")
                    {
                        user = "Usted";
                    }
                    Console.WriteLine(user + " no tiene fotos actualmente");
                }
            }
            else
            {
                Console.WriteLine("Usuario invalido");
            }
        }

        private static void ListComments(Socket socket)
        {
            Console.WriteLine("Indique de que foto desea ver los comentarios");
            var photo = Console.ReadLine();
            CommandPackage package = new CommandPackage(HeaderConstants.Request, CommandConstants.CommentList, photo);
            CommandProtocol.SendCommand(socket, package);

            CommandPackage response = CommandProtocol.RecieveCommand(socket);
            if (response.Command == CommandConstants.CommentList)
            {
                Console.WriteLine("Los comentarios son:");
                List<string> comments = CommandProtocol.RecieveList(socket);
                foreach(string comment in comments)
                {
                    Console.WriteLine(comment);
                }
            }
            else
            {
                Console.WriteLine("Nombre de foto invalido");
            }
        }

        private static void AddComment(Socket socket)
        {
            Console.WriteLine("Indique el usuario al cual pertenece la foto que desea comentar");
            var user = Console.ReadLine();
            Console.WriteLine("Indique la foto que desea comentar");
            var photo = Console.ReadLine();
            Console.WriteLine("Ingrese el comentario");
            var comment = Console.ReadLine();
            var data = user + "%" + photo + "%" + comment;

            CommandPackage package = new CommandPackage(HeaderConstants.Request, CommandConstants.NewComment, data);
            CommandProtocol.SendCommand(socket, package);

            CommandPackage response = CommandProtocol.RecieveCommand(socket);
            Console.WriteLine(response.Data);
        }
    }
}
