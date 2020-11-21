using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Obligatorio.LogsServer
{
    public class LogServer
    {
        public List<ILog> logs = new List<ILog>();
        private static LogServer instance;
        public bool running = true;

        public static LogServer GetInstance()
        {
            if (instance == null)
                instance = new LogServer();
            return instance;
        }

        public void StartListeners()
        {
            var infoListener = new Thread(() => QueueListener(LogConstants.Info, new Info()));
            infoListener.Start();
            var warningListener = new Thread(() => QueueListener(LogConstants.Warning, new Warning()));
            warningListener.Start();
            var errorListener = new Thread(() => QueueListener(LogConstants.Error, new Error()));
            errorListener.Start();
        }

        private static void QueueListener(string logType, ILog log)
        {
            while (LogServer.GetInstance().running)
            {
                var factory = new ConnectionFactory() { HostName = "localhost" };
                using(var connection = factory.CreateConnection())
                using(var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange: "topic_logs",
                                            type: "topic");
                    var queueName = channel.QueueDeclare().QueueName;
                    channel.QueueBind(queue: queueName,
                                      exchange: "topic_logs",
                                      routingKey: "log." + logType);
                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        var routingKey = ea.RoutingKey;
                        log.Message = message;
                        LogServer.GetInstance().logs.Add(log);
                    };
                }
            }
        }
    }
}
