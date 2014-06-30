using System;
using System.Collections.Generic;
using System.Text;
using Amazon.DataPipeline.Model;

namespace AWS.Core
{
    public class ConnectionInfo
    {
        public string PipelineId { get; set; }
        public string EndpointAddress { get; set; }
        public int EndpointPort { get; set; }
        public string PipelineName { get; set; }


        public ConnectionInfo()
        {
        }

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
                        MapToProperty(info, optionPairParts[0].Trim().ToLowerInvariant(), optionPairParts[1].Trim(), out info);
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

        public static void MapToProperty(ConnectionInfo existingInfo, List<Field> fields, out ConnectionInfo outInfo)
        {
            outInfo = existingInfo;
            foreach(var f in fields)
            {
                MapToProperty(outInfo, f.Key, f.StringValue, out outInfo);
            }
        }


        public static void MapToProperty(ConnectionInfo existingInfo, string key, string value, out ConnectionInfo outInfo)
        {
            outInfo = existingInfo;
            if ("dbinstanceidentifier".Equals(key))
            {
                outInfo.PipelineId = value;
                return;
            }

            if ("endpointaddress".Equals(key))
            {
                outInfo.EndpointAddress = value;
                return;
            }

            if ("endpointport".Equals(key))
            {
                int result;
                if (!int.TryParse(value, out result))
                {
                    throw new ArgumentException(string.Format("The connection info property '{0}' can only have an integer value but '{1}' was used instead.", key, value));
                }
                outInfo.EndpointPort = result;
                return;
            }

            throw new ArgumentException(string.Format("The connection info '{0}' was not expected and is not understood.", key));
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            if (PipelineId != null)
                builder.AppendFormat("DbInstanceIdentifier={0}&", PipelineId);

            if (EndpointAddress != null)
                builder.AppendFormat("EndpointAddress={0}&", EndpointAddress);

            if (EndpointPort != null)
                builder.AppendFormat("EndpointPort={0}&", EndpointPort);

            return builder.ToString(0, builder.Length - 1);
        }
    }
}