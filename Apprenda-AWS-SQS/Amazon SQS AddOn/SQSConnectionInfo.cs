namespace Apprenda.SaaSGrid.Addons.AWS.SQS
{
    using Newtonsoft.Json;

    public class SqsConnectionInfo
    {
        public string QueueName { get; set; }
        public string QueueUrl { get; set; }

        public static SqsConnectionInfo Parse(string _connectionInfo)
        {
            return JsonConvert.DeserializeObject<SqsConnectionInfo>(_connectionInfo);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}