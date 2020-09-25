using System;
using System.Collections.Generic;
using System.Text;
using Obligatorio.Exceptions;

namespace Obligatorio.ServerClient
{
    public class Server
    {
        private List<User> users = new List<User>();
        private List<Connection> connections = new List<Connection>();
        private static Server serverInstance;

        public static Server GetInstance()
        {
            if (serverInstance == null)
            {
                serverInstance = new Server();
            }
            return serverInstance;
        }

        public void RegisterUser(string username, string password)
        {
            User newUser = new User();
            newUser.Name = username;
            newUser.Password = password;
            if (users.Contains(newUser))
            {
                throw new UserAlreadyExistsException();
            }
            else
            {
                users.Add(newUser);
            }
        }
    }
}
