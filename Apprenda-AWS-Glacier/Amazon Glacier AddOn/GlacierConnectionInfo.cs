using System;
using System.Text;

namespace Apprenda.SaaSGrid.Addons.AWS.Glacier
{
    public class GlacierConnectionInfo
    {
        public string AccountId { get; set; }
        public string VaultName { get; set; }
        public string Location { get; set; }


        public static GlacierConnectionInfo Parse(string connectionInfo)
        {
            GlacierConnectionInfo info = new GlacierConnectionInfo();

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

        public static void MapToProperty(GlacierConnectionInfo existingInfo, string key, string value)
        {
            if ("accountid".Equals(key))
            {
                existingInfo.AccountId = value;
                return;
            }

            if ("vaultname".Equals(key))
            {
                existingInfo.VaultName = value;
                return;
            }

            if ("location".Equals(key))
            {
                existingInfo.Location = value;
                return;
            }

            throw new ArgumentException(string.Format("The connection info '{0}' was not expected and is not understood.", key));
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            if (AccountId != null)
                builder.AppendFormat("DbInstanceIdentifier={0}&", AccountId);

            if (VaultName != null)
                builder.AppendFormat("EndpointAddress={0}&", VaultName);

            if (Location != null)
                builder.AppendFormat("EndpointPort={0}&", Location);

            return builder.ToString(0, builder.Length - 1);
        }

        
    }
}