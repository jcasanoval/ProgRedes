using System;
using System.Collections.Generic;
using System.Text;
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
            Console.ReadLine();
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

        private void ReceiveMessages(IModel channel, string queueName)
        {
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                logs.Add(LogHandler.Deserialize(message));
                Console.WriteLine(message);
            };

            channel.BasicConsume(queueName, true, consumer);
        }


    }
}
