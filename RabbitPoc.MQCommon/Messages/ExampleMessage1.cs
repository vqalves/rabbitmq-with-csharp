using RabbitPoc.MQCommon.Structure;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitPoc.MQCommon.Messages
{
    public class ExampleMessage1
    {
        public const string TypeID = "ExampleMessage1";

        public string SentMessage { get; set; }
        public DateTime Date { get; set; }
        public Guid? Guid { get; set; }
    }
}