using System;
using System.Collections.Generic;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace LogServer
{
    class LogServer
    {
        private const string ExchangeName = "logExchange";

        private static List<String> logs = new List<String>();
        static void Main(string[] args)
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
            channel.ExchangeDeclare(ExchangeName, ExchangeType.Fanout);
            
        }

        private static void QueueBind(IModel channel, string queueName)
        {
            channel.QueueBind(
                queue: queueName,
                exchange: ExchangeName,
                routingKey: String.Empty);
        }

        private static void ReceiveMessages(IModel channel, string queueName)
        {
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                logs.Add(message);
                foreach (string log in logs) 
                {
                    Console.WriteLine("logs: " + log);
                }
            };

            channel.BasicConsume(queueName, true, consumer);
        }


    }
}
