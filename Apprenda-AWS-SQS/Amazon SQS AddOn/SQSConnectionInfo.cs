using System;
using System.Text;

namespace Apprenda.SaaSGrid.Addons.AWS.SQS
{
    using Newtonsoft.Json;

    public class SQSConnectionInfo
    {
        public string QueueName { get; set; }
        public string QueueURL { get; set; }

        public static SQSConnectionInfo Parse(string connectionInfo)
        {
            return JsonConvert.DeserializeObject<SQSConnectionInfo>(connectionInfo);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}