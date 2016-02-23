using Newtonsoft.Json;
using System;

namespace Apprenda.SaaSGrid.Addons.AWS.S3
{
    public class S3ConnectionInfo
    {
        public String BucketName { get; set; }

        public static S3ConnectionInfo Parse(string connectionInfo)
        {
            try
            {
                var info = JsonConvert.DeserializeObject<S3ConnectionInfo>(connectionInfo);
                return info;
            }
            catch (Exception)
            {
                throw new ArgumentException("Deserialization failed, check your provisioned addon instance data.");
            }
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}