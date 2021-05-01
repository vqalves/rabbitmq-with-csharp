using RabbitMQ.Client;

namespace RabbitPoc.MQCommon.Queues
{
    public class MainQueue
    {
        public const string QueueName = "MainQueue";
        public static QueueDeclareOk DeclareQueue(IModel model) => model.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
    }
}