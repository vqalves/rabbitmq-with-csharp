using RabbitMQ.Client;

namespace RabbitPoc.MQCommon.Queues
{
    public class RPCResponseQueue
    {
        public const string QueueName = "RPCResponseQueue";
        public static QueueDeclareOk DeclareQueue(IModel model) => model.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
    }
}