using RabbitPoc.MQCommon.Structure;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitPoc.MQCommon.Messages
{
    public class ExampleMessage2
    {
        public const string TypeID = "ExampleMessage2";

        public Guid Guid { get; set; }
    }
}