using Newtonsoft.Json;
using System;

namespace Apprenda.SaaSGrid.Addons.AWS.RDS
{
    public class RDSConnectionInfo
    {
        public string DbInstanceIdentifier { get; set; }

        public string EndpointAddress { get; set; }

        public int EndpointPort { get; set; }

        public static RDSConnectionInfo Parse(string connectionInfo)
        {
            try
            {
                var info = JsonConvert.DeserializeObject<RDSConnectionInfo>(connectionInfo);
                return info;
            }
            catch (Exception)
            {
                throw new ArgumentException(
                            string.Format("Deserialization failed of connection object. Check your provisioned addon instance data."));
            }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}