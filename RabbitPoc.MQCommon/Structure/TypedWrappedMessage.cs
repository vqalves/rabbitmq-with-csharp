using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitPoc.MQCommon.Structure
{
    public class TypedWrappedMessage
    {
        public string messageTypeId { get; set; }

        public static string GetType(string json)
        {
            return JsonConvert.DeserializeObject<TypedWrappedMessage>(json)?.messageTypeId;
        }
    }
}