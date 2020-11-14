using Common.CommandProtocol;
using Common.FileProtocol.Protocol;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
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
                        case CommandConstants.UserList:
                            UserList(client, connection);
                            break;
                        case CommandConstants.SendPicture:
                            RecievePicture(client, connection, package.Data);
                            break;
                        case CommandConstants.PictureList:
                            PictureList(client, connection, package.Data);
                            break;
                        case CommandConstants.CommentList:
                            CommentList(client, connection, package.Data);
                            break;
                        case CommandConstants.NewComment:
                            AddComment(client, connection, package.Data);
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

        private static void UserList(Socket client, Connection connection)
        {
            List<string> userList = Server.GetInstance().UserListStrings();
            CommandProtocol.SendList(client, userList, CommandConstants.UserList);
        }

        private static void RecievePicture(Socket client, Connection connection, String data)
        {
            FileProtocol fileProtocol = new FileProtocol(client);
            var fileName = fileProtocol.ReceiveFile(Server.pictureCount + "");
            Server.pictureCount ++;
            connection.User.UploadPicture(fileName, data);
        }

        private static void PictureList(Socket client, Connection connection, String data)
        {
            User user;
            if (data == "")
            {
                user = connection.User;
            }
            else
            {
                user = Server.GetInstance().GetUser(data);
            }
            CommandPackage response;
            
            if (user != null)
            {
                response = new CommandPackage(HeaderConstants.Response, CommandConstants.PictureList);
                CommandProtocol.SendCommand(client, response);

                CommandProtocol.SendList(client, user.PictureList(), CommandConstants.PictureList);
            }
            else
            {
                response = new CommandPackage(HeaderConstants.Response, CommandConstants.Error);
                CommandProtocol.SendCommand(client, response);
            }
        }

        private static void CommentList(Socket client, Connection connection, String data)
        {
            Photo photo = connection.User.GetPhoto(data);
            CommandPackage response;
            if (photo != null)
            {
                response = new CommandPackage(HeaderConstants.Response, CommandConstants.CommentList);
                CommandProtocol.SendCommand(client, response);

                CommandProtocol.SendList(client, photo.CommentList(), CommandConstants.CommentList);
            }
            else
            {
                response = new CommandPackage(HeaderConstants.Response, CommandConstants.Error);
                CommandProtocol.SendCommand(client, response);
            }
        }

        private static void AddComment(Socket client, Connection connection, String data)
        {
            var dataStruct = data.Split('%');
            User user = Server.GetInstance().GetUser(dataStruct[0]);
            CommandPackage response;
            if (user != null)
            {
                Photo photo = user.GetPhoto(dataStruct[1]);
                if (photo != null)
                {
                    Comment comment = new Comment();
                    comment.User = user;
                    comment.Text = dataStruct[2];
                    photo.Comments.Add(comment);
                    response = new CommandPackage(HeaderConstants.Response, CommandConstants.NewComment, "Comentario ingresado con exito");
                }
                else
                {
                    response = new CommandPackage(HeaderConstants.Response, CommandConstants.Error, "El usuario no contiene una foto con ese nombre");
                }
            }
            else
            {
                response = new CommandPackage(HeaderConstants.Response, CommandConstants.Error, "El usuario ingresado no es valido");
            }
            CommandProtocol.SendCommand(client, response);
        }
    }
}
