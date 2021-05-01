using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitPoc.MQCommon.Structure
{
    public abstract class IdentifiedMessage
    {
        public abstract string MessageTypeId();

        public bool IsType(string json) => MessageMethods.IsType(MessageTypeId(), json);
    }
}