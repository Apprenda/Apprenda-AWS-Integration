using System;
using System.Text;

namespace Apprenda.SaaSGrid.Addons.AWS.Glacier
{
    using Newtonsoft.Json;

    public class GlacierConnectionInfo
    {
        public string AccountId { get; set; }
        public string VaultName { get; set; }
        public string Location { get; set; }


        public static GlacierConnectionInfo Parse(string _connectionInfo)
        {
            return JsonConvert.DeserializeObject<GlacierConnectionInfo>(_connectionInfo);
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        
    }
}