using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Protocoles;

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
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                Console.WriteLine("se cerro la conexion");
            }
            catch (SocketException x)
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
                    keepRunning = false;
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
            Console.ReadLine();
        }

        
    }
}
