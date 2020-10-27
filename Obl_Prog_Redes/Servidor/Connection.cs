using Protocoles;
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
            while (connected)
            {
                try
                {
                    CommandPackage package = CommandProtocol.RecieveCommand(client);
                    switch (package.Command)
                    {
                        case CommandConstants.RequestLoggedUser:
                            RequestLoggedUser(client, connection);
                            break;
                        case CommandConstants.Login:
                            Login(client, package.Data, connection);
                            break;
                        case CommandConstants.Register:
                            Register(client, package.Data, connection);
                            break;
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

        private static void RequestLoggedUser(Socket client, Connection connection)
        {
            CommandPackage response;
            if (connection.User == null)
            {
                response = new CommandPackage(HeaderConstants.Response, CommandConstants.RequestLoggedUser, MessageConstants.NoUserFound);
            }
            else
            {
                response = new CommandPackage(HeaderConstants.Response, CommandConstants.RequestLoggedUser, connection.User.Name);
            }
            CommandProtocol.SendCommand(client, response);
        }

        private static void Login(Socket client, String data, Connection connection)
        {
            var message = data.Split("%");
            string username = message[0];
            string password = message[1];
            CommandPackage response;
            try
            {
                connection.User = Server.GetInstance().Login(username, password);
                response = new CommandPackage(HeaderConstants.Response, CommandConstants.Login, MessageConstants.SuccessfulLogin);
            }
            catch (Exception ex)
            {
                response = new CommandPackage(HeaderConstants.Response, CommandConstants.Login, MessageConstants.FailedLogin);
            }
            CommandProtocol.SendCommand(client, response);
        }

        private static void Register(Socket client, String data, Connection connection)
        {
            var message = data.Split("%");
            string username = message[0];
            string password = message[1];
            CommandPackage response;
            try
            {
                connection.User = Server.GetInstance().RegisterUser(username, password);
                response = new CommandPackage(HeaderConstants.Response, CommandConstants.Register, MessageConstants.SuccessfulRegister);
            }
            catch (Exception ex)
            {
                response = new CommandPackage(HeaderConstants.Response, CommandConstants.Register, MessageConstants.FailedRegister);
            }
            CommandProtocol.SendCommand(client, response);
        }
    }
}
