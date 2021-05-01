using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitPoc.MQCommon.Structure
{
    public abstract class DelayedQueueConfig : QueueConfig
    {
        protected QueueConfig TargetQueue { get; private set; }

        public DelayedQueueConfig(QueueConfig targetQueue)
        {
            this.TargetQueue = targetQueue;
        }
    }
}
