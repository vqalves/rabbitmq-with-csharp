using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitPoc.MQCommon.Structure
{
    public class MessageMethods
    {
        public static byte[] ToByte<T>(string messageTypeId, T obj)
        {
            return Encoding.UTF8.GetBytes(ToJson(messageTypeId, obj));
        }

        public static string ToJson<T>(string messageTypeId, T obj)
        {
            return JsonConvert.SerializeObject(new SerializedContent<T>()
            {
                messageTypeId = messageTypeId,
                messageBody = obj
            });
        }

        public static string ToJson(byte[] bytes)
        {
            return Encoding.UTF8.GetString(bytes);
        }

        public static string ToJson(ReadOnlyMemory<byte> bytes)
        {
            return ToJson(bytes.ToArray());
        }

        public static T FromByte<T>(ReadOnlyMemory<byte> bytes)
        {
            return FromByte<T>(bytes.ToArray());
        }

        public static T FromByte<T>(byte[] bytes)
        {
            return FromJson<T>(ToJson(bytes));
        }

        public static T FromJson<T>(string json)
        {
            var content = JsonConvert.DeserializeObject<SerializedContent<T>>(json);
            if (content == null) return default(T);
            return content.messageBody;
        }

        public static string GetType(string json)
        {
            return JsonConvert.DeserializeObject<SerializedType>(json)?.messageTypeId;
        }

        public static bool IsType(string messageTypeId, byte[] bytes)
        {
            return IsType(messageTypeId, ToJson(bytes));
        }

        public static bool IsType(string messageTypeId, string json)
        {
            return GetType(json) == messageTypeId;
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
