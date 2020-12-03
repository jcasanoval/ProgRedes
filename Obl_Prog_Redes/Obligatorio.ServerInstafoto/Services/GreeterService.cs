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
            ActionResponse response = ListUsers();
            response.Message = message;
            return response;
        }

        private static ActionResponse ListUsers()
        {
            ActionResponse response = new ActionResponse();
            foreach (string user in Server.GetInstance().UserListStrings())
            {
                response.Users.Add(user);
            }
            return response;
        }
    }
}
