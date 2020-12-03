using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Common.CommandProtocol;
using Common.Logs;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace LogServer
{
    public class LogServer
    {

        private const string ExchangeName = "logExchange";
        private static LogServer instance;
        public List<ILog> logs = new List<ILog>();
        static void Main(string[] args)
        {
            LogServer.GetInstance().Consumer();
        }

        public static LogServer GetInstance()
        {
            if (instance == null)
                instance = new LogServer();
            return instance;
        }

        public void Consumer()
        {
            var connectionFactory = new ConnectionFactory { HostName = "localhost" };
            using IConnection connection = connectionFactory.CreateConnection();
            using IModel channel = connection.CreateModel();
            ExchangeDeclare(channel);
            string queueName = channel.QueueDeclare().QueueName;
            QueueBind(channel, queueName);
            ReceiveMessages(channel, queueName);
            SearchConnections();
        }
        private static void ExchangeDeclare(IModel channel)
        {
            channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct);
            
        }

        private static void QueueBind(IModel channel, string queueName)
        {
            channel.QueueBind(
                queue: queueName,
                exchange: ExchangeName,
                routingKey: String.Empty);
        }

        public void SearchConnections()
        {
            Console.WriteLine("Servidor de logs");
            var server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var localEp = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 7000);
            server.Bind(localEp);
            server.Listen(100);
            while (true)
            {
                var client = server.Accept();
                var connection = new Thread(() => HandleAdmin(client));
                connection.Start();
            }

        }

        private static void HandleAdmin(Socket socket)
        {
            bool keepRunning = true;
            while (keepRunning)
            {
                try
                {
                    CommandPackage package = CommandProtocol.RecieveCommand(socket);
                    switch (package.Command)
                    {
                        case CommandConstants.Logs:
                            LogServer.GetInstance().AdminLog(socket, package);
                            break;
                        case CommandConstants.LogInfo:
                            LogServer.GetInstance().AdminInfo(socket, package);
                            break;
                        case CommandConstants.LogWarning:
                            LogServer.GetInstance().AdminWarning(socket, package);
                            break;
                        case CommandConstants.LogError:
                            LogServer.GetInstance().AdminError(socket, package);
                            break;

                    }
                }
                catch (Exception ex)
                {
                    keepRunning = false;
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
            }
        }

        private void AdminLog(Socket socket, CommandPackage package) {
            List<string> list = ReturnLastN(logs, int.Parse(package.Data));
            CommandProtocol.SendList(socket, list, package.Command);
        }

        private void AdminInfo(Socket socket, CommandPackage package) {
            List<ILog> infos = new List<ILog>();
            foreach (ILog log in logs)
            {
                if (log is Info)
                    infos.Add(log);
            }
            List<string> list = ReturnLastN(infos, int.Parse(package.Data));
            CommandProtocol.SendList(socket, list, package.Command);
        }

        private void AdminWarning(Socket socket, CommandPackage package) {
            List<ILog> warnings = new List<ILog>();
            foreach (ILog log in logs)
            {
                if (log is Warning)
                    warnings.Add(log);
            }
            List<string> list = ReturnLastN(warnings, int.Parse(package.Data));
            CommandProtocol.SendList(socket, list, package.Command);
        }

        private void AdminError(Socket socket, CommandPackage package) {
            List<ILog> errors = new List<ILog>();
            foreach (ILog log in logs)
            {
                if (log is Error)
                    errors.Add(log);
            }
            List<string> list = ReturnLastN(errors, int.Parse(package.Data));
            CommandProtocol.SendList(socket, list, package.Command);
        }

        private List<string> ReturnLastN(List<ILog> list, int n)
        {
            int ammount = Math.Min(n, list.Count);
            List<string> result = new List<string>();
            for (int i = list.Count - ammount; i < list.Count; i++)
            {
                ILog log = list[i];
                string type = "";
                if (log is Info)
                    type = "Info";
                if (log is Warning)
                    type = "Warning";
                if (log is Error)
                    type = "Error";
                string entry = type + ": \n Usuario " + log.User + "\n " + log.Message;
                result.Add(entry);
            }
            return result;
        }
        private void ReceiveMessages(IModel channel, string queueName)
        {
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                logs.Add(LogHandler.Deserialize(message));
            };

            channel.BasicConsume(queueName, true, consumer);
        }


    }
}
