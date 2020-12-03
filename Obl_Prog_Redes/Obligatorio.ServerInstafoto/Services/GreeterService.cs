using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Obligatorio.ServerInstafoto
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }

        public override Task<ActionResponse> RegisterUser(UserRpc user, ServerCallContext context)
        {
            return Task.FromResult(InsertUser(user));
        }

        private static ActionResponse InsertUser(UserRpc user)
        {
            string message;
            try
            {
                Server.GetInstance().RegisterUser(user.Name, user.Password);
                message = "Registro exitoso";
            }
            catch
            {
                message = "Nombre de usuario ya esta en uso";
            }
            ActionResponse response = new ActionResponse();
            response.Message = message;
            return response;
        }

        public override Task<ActionResponse> RemoveUser(UserRpc user, ServerCallContext context)
        {
            return Task.FromResult(DeleteUser(user));
        }

        private static ActionResponse DeleteUser(UserRpc user)
        {
            bool result = Server.GetInstance().DeleteUser(user.Name);
            ActionResponse response = new ActionResponse();
            if (result)
                response.Message = "Usuario eliminado correctamente";
            else
                response.Message = "No se encontro el usuario";
            return response;
        }

        public override Task<ActionResponse> ModifyUser(UserUpdate user, ServerCallContext context)
        {
            return Task.FromResult(UpdateUser(user));
        }

        private static ActionResponse UpdateUser(UserUpdate user)
        {
            ActionResponse response = new ActionResponse();
            response.Message = Server.GetInstance().ModUser(user.Name, user.NewName, user.NewPassword);
            return response;
        }

        public override Task<UserList> ListUsers(ListRequest request, ServerCallContext context)
        {
            return Task.FromResult(Users());
        }
        private static UserList Users()
        {
            UserList response = new UserList();
            foreach (string user in Server.GetInstance().UserListStrings())
            {
                response.Users.Add(user);
            }
            return response;
        }
    }
}
