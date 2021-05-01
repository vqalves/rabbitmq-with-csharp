using RabbitPoc.MQCommon;
using RabbitPoc.MQCommon.Exchanges;
using RabbitPoc.MQCommon.Messages;
using RabbitPoc.MQCommon.Queues;
using RabbitPoc.MQCommon.Structure;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitPoc.Principal.Business
{
    public class NotificacaoBusiness
    {
        private readonly ConnectionFactory factory;

        public NotificacaoBusiness()
        {
            this.factory = new ConnectionFactory() { HostName = "localhost" };
        }

        public void Enviar(string message)
        {
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "test", durable: true, exclusive: false, autoDelete: false, arguments: null);

                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "", routingKey: "test", basicProperties: null, body: body);

                Console.WriteLine(" [x] Sent {0}", message);
            }
        }

        public void EnviarAlt(string message)
        {
            var content = new ExampleMessage2()
            {
                Guid = Guid.NewGuid()
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                MainQueue.DeclareQueue(channel);

                var body = MessageMethods.ToByte(ExampleMessage2.TypeID, content);
                channel.BasicPublish(exchange: DefaultExchange.ExchangeName, routingKey: MainQueue.QueueName, basicProperties: null, body: body);
            }
        }

        public void EnviarComDelay(string message)
        {
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                string delayedQueue = "delayed-queue-1m";
                string targetQueue = "test";

                var queueArguments = new Dictionary<string, object>();
                queueArguments.Add("x-dead-letter-exchange", "");
                queueArguments.Add("x-dead-letter-routing-key", targetQueue);
                queueArguments.Add("x-message-ttl", 60000);

                var body = Encoding.UTF8.GetBytes(message);

                channel.QueueDeclare(queue: delayedQueue, durable: true, exclusive: false, autoDelete: true, arguments: queueArguments);
                channel.BasicPublish(exchange: "", routingKey: delayedQueue, basicProperties: null, body: body);
            }
        }

        public void EnviarComDelay2(string message)
        {
            var content = new ExampleMessage1()
            {
                Name = message,
                Date = DateTime.Now
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                DelayedBy5SecondsQueue.DeclareQueue(channel);

                var body = MessageMethods.ToByte(ExampleMessage1.TypeID, content);
                channel.BasicPublish(exchange: DefaultExchange.ExchangeName, routingKey: DelayedBy5SecondsQueue.QueueName, basicProperties: null, body: body);
            }
        }

        public ExampleResponse1 EnviarComRPC(string message)
        {
            var responseQueue = new BlockingCollection<ExampleResponse1>();

            var content = new ExampleMessage1()
            {
                Name = message,
                Date = DateTime.Now
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                // Make sure all queues are declared before trying to consume
                DelayedBy5SecondsQueue.DeclareQueue(channel);
                RPCResponseQueue.DeclareQueue(channel);

                // Activate a consumer before sending the message, to make sure the workers don't add a feedback before you're listening
                var correlationId = Guid.NewGuid().ToString("n");
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (ch, ea) =>
                {

                    System.Diagnostics.Debug.WriteLine($"F> {ea.BasicProperties.CorrelationId} / LF> {correlationId}");

                    if (ea.BasicProperties.CorrelationId == correlationId)
                    {
                        var response = MessageMethods.FromByte<ExampleResponse1>(ea.Body);
                        channel.BasicAck(ea.DeliveryTag, false);
                        responseQueue.Add(response);
                    }
                };

                channel.BasicConsume(RPCResponseQueue.QueueName, false, consumer);

                // Send the message
                var messageProperties = channel.CreateBasicProperties();
                messageProperties.CorrelationId = correlationId;
                messageProperties.ReplyTo = RPCResponseQueue.QueueName;

                System.Threading.Thread.Sleep(new Random().Next(0, 10000));

                var body = MessageMethods.ToByte(ExampleMessage1.TypeID, content);
                channel.BasicPublish(exchange: DefaultExchange.ExchangeName, routingKey: DelayedBy5SecondsQueue.QueueName, basicProperties: messageProperties, body: body);

                // Any call that blocks the thread until the response is received works
                return responseQueue.Take();
            }
        }
    }
}
