using System;
using System.Text;

namespace Apprenda.SaaSGrid.Addons.AWS.SNS
{
    public class SnsConnectionInfo
    {
        public string TopicArn { get; set; }
        public string QueueName { get; set; }
        public static SnsConnectionInfo Parse(string _connectionInfo)
        {
            SnsConnectionInfo info = new SnsConnectionInfo();

            if (!string.IsNullOrWhiteSpace(_connectionInfo))
            {
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
            }

            return info;
        }

        public static void MapToProperty(SnsConnectionInfo _existingInfo, string _key, string _value)
        {
            if ("TopicArn".Equals(_key))
            {
                _existingInfo.TopicArn = _value;
                return;
            }

            if ("QueueName".Equals(_key))
            {
                _existingInfo.QueueName = _value;
                return;
            }
            throw new ArgumentException(string.Format("The connection info '{0}' was not expected and is not understood.", _key));
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            if (TopicArn != null)
                builder.AppendFormat("DbInstanceIdentifier={0}&", TopicArn);

            if (QueueName != null)
                builder.AppendFormat("EndpointAddress={0}&", QueueName);

            return builder.ToString(0, builder.Length - 1);
        }

        
    }
}