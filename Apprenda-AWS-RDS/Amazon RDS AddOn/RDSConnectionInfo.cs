using Newtonsoft.Json;
using System;

namespace Apprenda.SaaSGrid.Addons.AWS.RDS
{
    public class RDSConnectionInfo
    {
        private static string ConnectionStringTemplate = "Data Source={0};Database={1};User Id={2};Password={3};MultipleActiveResultSets=True";
        public string DbInstanceIdentifier { get; set; }

        public string EndpointAddress { get; set; }

        //public int EndpointPort { get; set; }
        public string MasterUsername { get; set; }
        public string MasterPassword { get; set; }

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
            return string.Format(ConnectionStringTemplate, EndpointAddress, DbInstanceIdentifier, MasterUsername, MasterPassword);
            //return JsonConvert.SerializeObject(this);
        }
    }
}