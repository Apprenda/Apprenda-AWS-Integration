using Newtonsoft.Json;
using System;

namespace Apprenda.SaaSGrid.Addons.AWS.S3
{
    public class S3ConnectionInfo
    {
        public string BucketName { get; set; }

        public static S3ConnectionInfo Parse(string _connectionInfo)
        {
            try
            {
                // var info = JsonConvert.DeserializeObject<S3ConnectionInfo>(_connectionInfo);
                return new S3ConnectionInfo() { BucketName = _connectionInfo };
            }
            catch (Exception)
            {
                throw new ArgumentException("Parse failed, check your provisioned addon instance data.");
            }
        }

        public override string ToString()
        {
            return this.BucketName;
        }
    }
}