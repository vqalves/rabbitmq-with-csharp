using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitPoc.MQCommon.Structure
{
    public class WrappedMessage
    {
        private WrappedMessage() { }

        public static WrappedMessage<T> Wrap<T>(string messageTypeId, T obj)
        {
            return new WrappedMessage<T>() 
            { 
                messageTypeId = messageTypeId, 
                messageBody = obj 
            };
        }

        public static string ToJson(ReadOnlyMemory<byte> bytes)
        {
            var byteArray = bytes.ToArray();
            return Encoding.UTF8.GetString(byteArray);
        }
    }

    public class WrappedMessage<T> : TypedWrappedMessage
    {
        public T messageBody { get; set; }

        public byte[] ToBytes()
        {
            var json = JsonConvert.SerializeObject(this);
            return Encoding.UTF8.GetBytes(json);
        }

        public static T Unwrap(ReadOnlyMemory<byte> bytes)
        {
            string json = WrappedMessage.ToJson(bytes);
            return Unwrap(json);
        }

        public static T Unwrap(string json)
        {
            var obj = JsonConvert.DeserializeObject<WrappedMessage<T>>(json);

            if (obj == null)
                return default(T);

            return obj.messageBody;
        }
    }
}
