using System;
using System.Text;

namespace Apprenda.SaaSGrid.Addons.AWS.SQS
{
    public class ConnectionInfo
    {
        public String queueName { get; set; }
        public String queueURL { get; set; }

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
            if ("queueName".Equals(key))
            {
                existingInfo.queueName = value;
                return;
            }

            if ("queueURL".Equals(key))
            {
                existingInfo.queueURL = value;
                return;
            }
            throw new ArgumentException(string.Format("The connection info '{0}' was not expected and is not understood.", key));
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            if (queueName != null)
                builder.AppendFormat("DbInstanceIdentifier={0}&", queueName);

            if (queueURL != null)
                builder.AppendFormat("EndpointAddress={0}&", queueURL);
            return builder.ToString(0, builder.Length - 1);
        }
    }
}