using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RabbitPoc.MQCommon.Structure
{
    public class ProcessMessageDictionary
    {
        private Dictionary<string, Func<string, Task<byte[]>>> ExecuteByType;

        public ProcessMessageDictionary()
        {
            this.ExecuteByType = new Dictionary<string, Func<string, Task<byte[]>>>();
        }

        public void Register<T>(string messageTypeId, Func<T, Task<byte[]>> doWork)
        {
            this.ExecuteByType.Add
            (
                messageTypeId,
                async (json) => await doWork(MessageMethods.FromJson<T>(json))
            );
        }

        public async Task<ExecutionResult> Execute(string json)
        {
            var result = new ExecutionResult();
            var type = MessageMethods.GetType(json);

            if (ExecuteByType.TryGetValue(type, out var doWork))
            {
                result.Result = await doWork(json);
                result.Processed = true;
            }
            else
            {
                result.Result = null;
                result.Processed = false;
            }

            return await Task.FromResult(result);
        }

        public class ExecutionResult
        {
            public bool Processed;
            public byte[] Result;
        }


        /*
        private Dictionary<string, Action<string>> ExecuteByType;

        public ProcessMessageDictionary()
        {
            this.ExecuteByType = new Dictionary<string, Action<string>>();
        }

        public void Register<T>(string messageTypeId, Action<T> doWork)
        {
            this.ExecuteByType.Add
            (
                messageTypeId,
                (json) => doWork(MessageMethods.FromJson<T>(json))
            );
        }

        public bool Execute(string json)
        {
            var type = MessageMethods.GetType(json);

            if (ExecuteByType.TryGetValue(type, out var doWork))
            {
                doWork(json);
                return true;
            }

            return false;
        }
        */
    }
}
