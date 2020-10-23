using System;
using System.Net.Sockets;
using System.Net;
using System.Text;

namespace Client
{
    class Client
    {
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
                    MainMenu(isRunning);
                }
            }
            catch (SocketException x)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                Console.WriteLine("se cerro la conexion");
            }
        }

        private static void MainMenu(bool keepRunning)
        {
            //hacer request para ver si hay usuario logueado
            //segun el caso llamar loginmainmenu o loggedmainmenu
        }

        private static void LoginMainMenu(bool keepRunning)
        {
            Console.WriteLine("Bienvenido al menu principal, ingresa a tu cuenta o registrate \n 1-Login \n 2-Register \n 3-Exit");
            var input = Console.ReadLine();
            switch (input)
            {
                case "1":
                    LoginMenu();
                    break;
                case "2":
                    RegisterMenu();
                    break;
                case "3":
                    keepRunning = false;
                    break;
            }
        }

        private static void LoginMenu()
        {
            //menu login
        }

        private static void RegisterMenu()
        {
            //menu register
        }

        private static void LoggedMainMenu()
        {

        }

        
    }
}
