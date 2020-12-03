using Common.CommandProtocol;
using Common.FileProtocol.Protocol;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using RabbitMQ.Client;
using Common;
using Common.Logs;

namespace Obligatorio.ServerInstafoto
{
    public class Connection
    {
        public User User { get; set; }
        public Socket Socket { get; set; }
        static int _clientNumber;
        static object photoLocker;
        

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
                        case CommandConstants.Exit:
                            connected = false;
                            Console.WriteLine("El cliente " + id + " cerró la conexión: ");
                            break;
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
                        case CommandConstants.Logout:
                            Logout(connection);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("El cliente " + id + " cerró la conexión: ");
                    connected = false;
                    if (Server.keepRunning)
                    {
                        client.Shutdown(SocketShutdown.Both);
                        client.Close();
                    }
                    else
                    {
                        Console.WriteLine("server is shuting down");
                    }
                }
            }
            Server.GetInstance().connections.Remove(connection);
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
            ILog log;
            try
            {
                connection.User = Server.GetInstance().Login(username, password);
                response = new CommandPackage(HeaderConstants.Response, CommandConstants.Login, MessageConstants.SuccessfulLogin);
                log = new Info();
                log.User = username;
                log.Message = "Login exitoso";
            }
            catch (Exception ex)
            {
                response = new CommandPackage(HeaderConstants.Response, CommandConstants.Login, MessageConstants.FailedLogin);
                log = new Warning();
                log.User = username;
                log.Message = "Login no valido";
            }
            Server.GetInstance().LogAction(log);
            CommandProtocol.SendCommand(client, response);
        }

        private static void Logout(Connection connection)
        {
            ILog log = new Info();
            log.User = connection.User.Name;
            log.Message = "Usuario cierra sesion";
            Server.GetInstance().LogAction(log);
            connection.User = null;
        }

        private static void Register(Socket client, String data, Connection connection)
        {
            //lock (registerLocker)
            //{
                var message = data.Split("%");
                string username = message[0];
                string password = message[1];
                CommandPackage response;
                ILog log;
                try
                {
                    connection.User = Server.GetInstance().RegisterUser(username, password);
                    response = new CommandPackage(HeaderConstants.Response, CommandConstants.Register, MessageConstants.SuccessfulRegister);
                    log = new Info();
                    log.User = username;
                    log.Message = "Usuario registrado correctamente";
                }
                catch (Exception ex)
                {
                    response = new CommandPackage(HeaderConstants.Response, CommandConstants.Register, MessageConstants.FailedRegister);
                    log = new Warning();
                    log.User = username;
                    log.Message = "Registro no valido, nombre de usuario en uso";
                }
                Server.GetInstance().LogAction(log);
                CommandProtocol.SendCommand(client, response);
           // }
        }

        private static void UserList(Socket client, Connection connection)
        {
            List<string> userList = Server.GetInstance().UserListStrings();
            CommandProtocol.SendList(client, userList, CommandConstants.UserList);
            ILog log = new Info();
            log.User = connection.User.Name;
            log.Message = "Usuario solicita lista de usuarios";
            Server.GetInstance().LogAction(log);
        }

        private static void RecievePicture(Socket client, Connection connection, String data)
        {
            //lock (photoLocker)
            //{
            ILog log;
            try
            {
                FileProtocol fileProtocol = new FileProtocol(client);
                var fileName = fileProtocol.ReceiveFile(Server.pictureCount + "");
                Server.pictureCount++;
                connection.User.UploadPicture(fileName, data);
                log = new Info();
                log.User = connection.User.Name;
                log.Message = "Usuario sube nueva foto";
            }
            catch
            {
                log = new Error();
                log.User = connection.User.Name;
                log.Message = "Error subiendo foto";
            }
            Server.GetInstance().LogAction(log);
            //}
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
            ILog log;
            if (user != null)
            {
                response = new CommandPackage(HeaderConstants.Response, CommandConstants.PictureList);
                CommandProtocol.SendCommand(client, response);
                CommandProtocol.SendList(client, user.PictureList(), CommandConstants.PictureList);

                log = new Info();
                log.User = connection.User.Name;
                log.Message = "Usuario solicita fotos de " + user.Name;
            }
            else
            {
                response = new CommandPackage(HeaderConstants.Response, CommandConstants.Error);
                CommandProtocol.SendCommand(client, response);
                log = new Warning();
                log.User = connection.User.Name;
                log.Message = "Solicita fotos de usuario no valido";
            }
            Server.GetInstance().LogAction(log);
        }

        private static void CommentList(Socket client, Connection connection, String data)
        {
            Photo photo = connection.User.GetPhoto(data);
            CommandPackage response;
            ILog log;
            if (photo != null)
            {
                response = new CommandPackage(HeaderConstants.Response, CommandConstants.CommentList);
                CommandProtocol.SendCommand(client, response);
                CommandProtocol.SendList(client, photo.CommentList(), CommandConstants.CommentList);

                log = new Info();
                log.User = connection.User.Name;
                log.Message = "Usuario solicita comentarios de la foto " + photo.Name;
            }
            else
            {
                response = new CommandPackage(HeaderConstants.Response, CommandConstants.Error);
                CommandProtocol.SendCommand(client, response);
                log = new Warning();
                log.User = connection.User.Name;
                log.Message = "Solicita comentarios de foto no valida";
            }
            Server.GetInstance().LogAction(log);
        }

        private static void AddComment(Socket client, Connection connection, String data)
        {
            var dataStruct = data.Split('%');
            User user = Server.GetInstance().GetUser(dataStruct[0]);
            CommandPackage response;
            ILog log;
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
                    log = new Info();
                    log.User = connection.User.Name;
                    log.Message = "Usuario comenta la foto " + photo.Name + " del usuario " + user.Name;
                }
                else
                {
                    response = new CommandPackage(HeaderConstants.Response, CommandConstants.Error, "El usuario no contiene una foto con ese nombre");
                    log = new Warning();
                    log.User = connection.User.Name;
                    log.Message = "Intenta comentar una foto inexistente del usuario " + user.Name;
                }
            }
            else
            {
                response = new CommandPackage(HeaderConstants.Response, CommandConstants.Error, "El usuario ingresado no es valido");
                log = new Warning();
                log.User = connection.User.Name;
                log.Message = "Intenta comentar una foto de usuario no existente";
            }
            CommandProtocol.SendCommand(client, response);
            Server.GetInstance().LogAction(log);
        }
    }
}
