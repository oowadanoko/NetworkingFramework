using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AnotherTestUsingNamespace
{
    public class EchoMessage : Oowada.NetworkingFramework.Common.BaseMessage
    {
        [JsonInclude]
        public string data = "";

        public EchoMessage(string data)
        {
            this.data = data;
        }
    }
}
