using System;
using System.Text;

namespace Apprenda.SaaSGrid.Addons.AWS.RDS
{
    public class ConnectionInfo
    {
        public string DbInstanceIdentifier { get; set; }
        public string EndpointAddress { get; set; }
        public int? EndpointPort { get; set; }

        public static ConnectionInfo Parse(string connectionInfo)
        {
            ConnectionInfo info = new ConnectionInfo();

            if (!string.IsNullOrWhiteSpace(connectionInfo))
            {
                var propertyPairs = connectionInfo.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
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
            }

            return info;
        }

        public static void MapToProperty(ConnectionInfo existingInfo, string key, string value)
        {
            if ("dbinstanceidentifier".Equals(key))
            {
                existingInfo.DbInstanceIdentifier = value;
                return;
            }

            if ("endpointaddress".Equals(key))
            {
                existingInfo.EndpointAddress = value;
                return;
            }

            if ("endpointport".Equals(key))
            {
                int result;
                if (!int.TryParse(value, out result))
                {
                    throw new ArgumentException(string.Format("The connection info property '{0}' can only have an integer value but '{1}' was used instead.", key, value));
                }
                existingInfo.EndpointPort = result;
                return;
            }

            throw new ArgumentException(string.Format("The connection info '{0}' was not expected and is not understood.", key));
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            if (DbInstanceIdentifier != null)
                builder.AppendFormat("DbInstanceIdentifier={0}&", DbInstanceIdentifier);

            if (EndpointAddress != null)
                builder.AppendFormat("EndpointAddress={0}&", EndpointAddress);

            if (EndpointPort.HasValue)
                builder.AppendFormat("EndpointPort={0}&", EndpointPort);

            return builder.ToString(0, builder.Length - 1);
        }
    }
}