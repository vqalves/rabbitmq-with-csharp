using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitPoc.MQCommon.Exchanges;
using RabbitPoc.MQCommon.Messages;
using RabbitPoc.MQCommon.Queues;
using RabbitPoc.MQCommon.Structure;
using System;
using System.Collections.Concurrent;

namespace RabbitPoc.Principal.Business
{
    public class MQService
    {
        private readonly ConnectionFactory factory;

        public MQService()
        {
            // Factory can be attributed through DI
            this.factory = new ConnectionFactory() { HostName = "localhost" };
        }

        public void SendExampleMessage2()
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

        public void SendExampleMessage1_WithDelay(string name)
        {
            var content = new ExampleMessage1()
            {
                SentMessage = name,
                Date = DateTime.Now
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                // Delay configuration is done inside the queue declaration
                DelayedBy5SecondsQueue.DeclareQueue(channel);

                var body = MessageMethods.ToByte(ExampleMessage1.TypeID, content);
                channel.BasicPublish(exchange: DefaultExchange.ExchangeName, routingKey: DelayedBy5SecondsQueue.QueueName, basicProperties: null, body: body);
            }
        }

        public ExampleResponse1 SendAndWaitResponse_WithRPC(string message)
        {
            var responseQueue = new BlockingCollection<ExampleResponse1>();

            var content = new ExampleMessage1()
            {
                SentMessage = message,
                Date = DateTime.Now
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                DelayedBy5SecondsQueue.DeclareQueue(channel);

                // Create a new queue to await the results. The created queue has a unique name, is temporary and can be consumed exclusively to this channel. That way we can avoid other channels "stealing" the results of this call
                var temporaryResponseQueue = channel.QueueDeclare(exclusive: true);
                var responseQueueName = temporaryResponseQueue.QueueName;

                // CorrelationID should be a unique code, shared between the send and returned message
                var correlationId = Guid.NewGuid().ToString("n");

                // Send the message
                var messageProperties = channel.CreateBasicProperties();
                messageProperties.CorrelationId = correlationId;
                messageProperties.ReplyTo = responseQueueName;

                var body = MessageMethods.ToByte(ExampleMessage1.TypeID, content);
                channel.BasicPublish(exchange: DefaultExchange.ExchangeName, routingKey: DelayedBy5SecondsQueue.QueueName, basicProperties: messageProperties, body: body);

                // Listen to the response queue to receive the result data
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (ch, ea) =>
                {
                    if (ea.BasicProperties.CorrelationId == correlationId)
                    {
                        var response = MessageMethods.FromByte<ExampleResponse1>(ea.Body);
                        channel.BasicAck(ea.DeliveryTag, false);
                        responseQueue.Add(response);
                    }
                };

                channel.BasicConsume(responseQueueName, false, consumer);

                // Any call that halts the execution until the response is received works
                return responseQueue.Take();
            }
        }
    }
}
