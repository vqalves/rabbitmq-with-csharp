using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitPoc.MQCommon.Structure
{
    public abstract class QueueConfig
    {
        public abstract string QueueName { get; }

        public abstract QueueDeclareOk DeclareQueue(IModel model);
    }
}
