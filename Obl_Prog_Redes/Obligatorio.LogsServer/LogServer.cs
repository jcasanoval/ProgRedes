using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Logs;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Obligatorio.LogsServer
{
    public class LogServer
    {
        public List<ILog> logs = new List<ILog>();
        private static LogServer instance;
        public bool running = true;
        private const string ExchangeName = "logExchange";
        public IModel channel;

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
            channel = connection.CreateModel();
            ExchangeDeclare(channel);
            string queueName = channel.QueueDeclare().QueueName;
            QueueBind(queueName); 
            ReceiveMessages(queueName);
        }

        private static void ExchangeDeclare(IModel channel)
        {
            channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct);

        }

        private static void QueueBind(string queueName)
        {
            LogServer.GetInstance().channel.QueueBind(
                queue: queueName,
                exchange: ExchangeName,
                routingKey: String.Empty);
        }

        private void ReceiveMessages(string queueName)
        {
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                LogServer.GetInstance().logs.Add(LogHandler.Deserialize(message));
            };

            channel.BasicConsume(queueName, true, consumer);
        }
    }
}
