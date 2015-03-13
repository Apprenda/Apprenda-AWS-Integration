using Newtonsoft.Json;
using System;

namespace Apprenda.SaaSGrid.Addons.AWS.Redshift
{
    public class RedshiftConnectionInfo
    {
        public string ClusterIdentifier { get; set; }

        public string EndpointAddress { get; set; }

        public int EndpointPort { get; set; }

        public static RedshiftConnectionInfo Parse(string connectionInfo)
        {
            try
            {
                var info = JsonConvert.DeserializeObject<RedshiftConnectionInfo>(connectionInfo);
                return info;
            }
            catch (Exception)
            {
                throw new ArgumentException("Unable to deserialize Redshift Connection parameters, check provisioned addon instance data.");
            }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}