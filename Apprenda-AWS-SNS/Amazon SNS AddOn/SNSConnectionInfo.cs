using System;
using System.Text;

namespace Apprenda.SaaSGrid.Addons.AWS.SNS
{
    using Newtonsoft.Json;

    public class SnsConnectionInfo
    {
        public string TopicArn { get; set; }
        public string QueueName { get; set; }

        public static SnsConnectionInfo Parse(string _connectionInfo)
        {
            return JsonConvert.DeserializeObject<SnsConnectionInfo>(_connectionInfo);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        
    }
}