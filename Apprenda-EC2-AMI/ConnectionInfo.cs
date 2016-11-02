using System;
using System.Text;

namespace Apprenda.SaaSGrid.Addons.AWS.EC2
{
    public class ConnectionInfo
    {
        private string BucketName { get; set; }

        public static ConnectionInfo Parse(string _connectionInfo)
        {
            var info = new ConnectionInfo();

            if (string.IsNullOrWhiteSpace(_connectionInfo)) return info;
            var propertyPairs = _connectionInfo.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var propertyPair in propertyPairs)
            {
                var optionPairParts = propertyPair.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                if (optionPairParts.Length == 2)
                {
                    MapToProperty(info, optionPairParts[0].Trim().ToLowerInvariant(), optionPairParts[1].Trim());
                }
                else
                {
                    throw new ArgumentException(
                        string.Format(
                            "Unable to parse connection info which should be in the form of 'property=value&nextproperty=nextValue'. The property '{0}' was not properly constructed",
                            propertyPair));
                }
            }
            return info;
        }

        private static void MapToProperty(ConnectionInfo _existingInfo, string _key, string _value)
        {
            if (!"bucketname".Equals(_key))
                throw new ArgumentException(
                    string.Format("The connection info '{0}' was not expected and is not understood.", _key));
            _existingInfo.BucketName = _value;
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            if (this.BucketName != null) builder.AppendFormat("BucketName={0}", this.BucketName);
            return builder.ToString();
        }
    }
}