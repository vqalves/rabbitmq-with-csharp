using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitPoc.MQCommon.Structure
{
    public class MessageMethods
    {
        public static byte[] ToByte(string json) => Encoding.UTF8.GetBytes(json);
        public static string ToJson(byte[] bytes) => Encoding.UTF8.GetString(bytes);

        public static byte[] ToByte<T>(string messageTypeId, T obj)
        {
            var json = ToJson(messageTypeId, obj);
            var bytes = ToByte(json);
            return bytes;
        }

        public static string ToJson<T>(string messageTypeId, T obj)
        {
            return JsonConvert.SerializeObject(new SerializedContent<T>()
            {
                messageTypeId = messageTypeId,
                messageBody = obj
            });
        }

        public static string ToJson(ReadOnlyMemory<byte> bytes)
        {
            var byteArray = bytes.ToArray();
            return ToJson(byteArray);
        }

        public static T FromJson<T>(string json)
        {
            var content = JsonConvert.DeserializeObject<SerializedContent<T>>(json);
            if (content == null) return default(T);
            return content.messageBody;
        }

        public static T FromByte<T>(ReadOnlyMemory<byte> bytes)
        {
            var byteArray = bytes.ToArray();
            var json = ToJson(byteArray);
            var obj = FromJson<T>(json);

            return obj;
        }

        public static string GetType(string json)
        {
            return JsonConvert.DeserializeObject<SerializedType>(json)?.messageTypeId;
        }

        private class SerializedType
        {
            public string messageTypeId { get; set; }
        }

        private class SerializedContent<T> : SerializedType
        {
            public T messageBody { get; set; }
        }
    }
}
