using System;
using Newtonsoft.Json;

namespace Apprenda.SaaSGrid.Addons.AWS.EMR
{
    public class EMRConnectionInfo
    {
        public string ClusterIdentifier { get; set; }
        public string EndpointAddress { get; set; }
        public int? EndpointPort { get; set; }

        public static EMRConnectionInfo Parse(string connectionInfo)
        {
            return JsonConvert.DeserializeObject<EMRConnectionInfo>(connectionInfo);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}