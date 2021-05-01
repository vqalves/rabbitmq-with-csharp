using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitPoc.MQCommon.Messages
{
    public class ExampleResponse1
    {
        public const string TypeID = "ExampleResponse1";


        public ExampleMessage1 ProcessedMessage { get; set; }
        public DateTime ProcessedDate { get; set; }
    }
}
