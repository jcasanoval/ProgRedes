using Common.CommandProtocol;
using Grpc.Net.Client;
using Obligatorio.ServerInstafoto;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using static Obligatorio.ServerInstafoto.Greeter;

namespace Obligatorio.AdminClient
{
    class AdminClient
    {
        private const String separador = "---------------------------------------------------------------";
        static async Task Main(string[] args)
        {
            var isRunning = true;
            var clientEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1);
            var serverEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7000);
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(clientEndpoint);
            socket.Connect(serverEndpoint);
            GrpcChannel channel = GrpcChannel.ForAddress("https://localhost:5001");
            Greeter.GreeterClient client = new Greeter.GreeterClient(channel);
            Console.WriteLine("conectado");
            try
            {
                while (isRunning)
                {
                    Menu(ref isRunning, socket, client);
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

        private static void Menu(ref bool keepRunning, Socket socket, GreeterClient client)
        {
            Console.WriteLine(separador);
            Console.WriteLine("Bienvenido al menu principal de administrador, ingresa la opcion que deseas realizar \n 1-ABM usuario \n 2-Ver logs \n 3-Exit");
            var input = Console.ReadLine();
            switch (input)
            {
                case "1":
                    ABMMenuAsync(client);
                    break;
                case "2":
                    LogMenu(socket);
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

        private static async Task ABMMenuAsync(GreeterClient client)
        {
            Console.WriteLine(separador);
            Console.WriteLine("Ingresa que tipo de operacion deseas realizar \n 1-Alta \n 2-Baja \n 3-Modificacion");
            var input = Console.ReadLine();
            switch (input)
            {
                case "1":
                    Console.WriteLine("Inserte nombre de usuario");
                    string name = Console.ReadLine();
                    while (name.Trim().Equals(""))
                    {
                        name = Console.ReadLine();
                    }
                    Console.WriteLine("Inserte contraseña");
                    string password = Console.ReadLine();
                    while (password.Trim().Equals(""))
                    {
                        password = Console.ReadLine();
                    }
                        var user = new UserRpc
                        {
                            Name = name,
                            Password = password
                        };
                        var response = await client.RegisterUserAsync(user);
                        Console.WriteLine(response.Message);
                    break;
                case "2":
                    break;
                case "3":
                    break;
                default:
                    break;
            }
        }

        private static void LogMenu(Socket socket)
        {
            Console.WriteLine(separador);
            Console.WriteLine("Ingresa que tipo de logs deseas ver \n 1-Todos los logs \n 2-Info \n 3-Warning \n 4-Error");
            var input = Console.ReadLine();
            List<string> result = new List<string>();
            CommandPackage request;
            string ammount;
            switch (input)
            {
                case "1":
                    Console.WriteLine("Ingrese la cantidad de logs que desea ver");
                    ammount = Console.ReadLine();
                    request = new CommandPackage(HeaderConstants.Request, CommandConstants.Logs, ammount);
                    CommandProtocol.SendCommand(socket, request);
                    result = CommandProtocol.RecieveList(socket);
                    break;
                case "2":
                    Console.WriteLine("Ingrese la cantidad de logs que desea ver");
                    ammount = Console.ReadLine();
                    request = new CommandPackage(HeaderConstants.Request, CommandConstants.LogInfo, ammount);
                    CommandProtocol.SendCommand(socket, request);
                    result = CommandProtocol.RecieveList(socket);
                    break;
                case "3":
                    Console.WriteLine("Ingrese la cantidad de logs que desea ver");
                    ammount = Console.ReadLine();
                    request = new CommandPackage(HeaderConstants.Request, CommandConstants.LogWarning, ammount);
                    CommandProtocol.SendCommand(socket, request);
                    result = CommandProtocol.RecieveList(socket);
                    break;
                case "4":
                    Console.WriteLine("Ingrese la cantidad de logs que desea ver");
                    ammount = Console.ReadLine();
                    request = new CommandPackage(HeaderConstants.Request, CommandConstants.LogError, ammount);
                    CommandProtocol.SendCommand(socket, request);
                    result = CommandProtocol.RecieveList(socket);
                    break;
                default:
                    Console.WriteLine("Opcion no valida");
                    break;
            }
            if (result.Count > 0)
            {
                foreach (string log in result)
                    Console.WriteLine(log);
            }
        }
    }
}
