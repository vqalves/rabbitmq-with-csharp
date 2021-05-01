using RabbitPoc.MQCommon.Exchanges;
using RabbitMQ.Client;
using System.Collections.Generic;

namespace RabbitPoc.MQCommon.Queues
{
    public class DelayedBy5SecondsQueue
    {
        public const string QueueName = "DelayedBy5SecondsQueue";
        public static QueueDeclareOk DeclareQueue(IModel model)
        {
            // Make sure the target queue is declared
            MainQueue.DeclareQueue(model);

            var queueArguments = new Dictionary<string, object>();
            queueArguments.Add("x-dead-letter-exchange", DefaultExchange.ExchangeName);
            queueArguments.Add("x-dead-letter-routing-key", MainQueue.QueueName); // Target queue
            queueArguments.Add("x-message-ttl", 5000); // Delay in miliseconds

            return model.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: true, arguments: queueArguments);
        }
    }
}