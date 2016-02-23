using System;
using System.Collections.Generic;

namespace Apprenda.SaaSGrid.Addons.AWS.Redshift
{
    public class RedshiftDeveloperOptions
    {
        public string ClusterParameterGroupName { get; private set; }

        public string MasterUserName { get; private set; }

        public string MasterPassword { get; private set; }

        public string AccessKey { get; private set; }

        public string SecretAccessKey { get; private set; }

        // we need to put RegionEndpoint back in there TODO
        public string RegionEndpoint { get; set; }

        // Amazon Redshift Options
        public int AllocatedStorage { get; private set; }

        public string AvailabilityZone { get; private set; }

        public bool AllowVersionUpgrade { get; private set; }

        public int AutomatedSnapshotRetentionPeriod { get; private set; }

        public string ClusterIdentifier { get; private set; }

        public List<string> ClusterSecurityGroups { get; set; }

        public string ClusterSubnetGroupName { get; private set; }

        public string ClusterType { get; private set; }

        public string ClusterVersion { get; private set; }

        public string DbName { get; private set; }

        public string ElasticIp { get; private set; }

        public bool Encrypted { get; private set; }

        public string HsmClientCertificateIdentifier { get; private set; }

        public string HsmClientConfigurationIdentifier { get; private set; }

        public string NodeType { get; private set; }

        public int NumberOfNodes { get; private set; }

        public int Port { get; private set; }

        public string PreferredMaintenanceWindow { get; private set; }

        public bool PubliclyAccessible { get; private set; }

        public List<string> VpcSecurityGroupIds { get; set; }

        public static RedshiftDeveloperOptions Parse(IEnumerable<AddonParameter> developerParameters)
        {
            // we made this so much easier with developer parameters.

            var options = new RedshiftDeveloperOptions();

            foreach (var parameter in developerParameters)
            {
                MapToOption(options, parameter.Key.ToLowerInvariant(), parameter.Value);
            }
            return options;
        }

        // Interior method takes in instance of DeveloperOptions (aptly named existingOptions) and maps them to the proper value. In essence, a setter method.
        public static void MapToOption(RedshiftDeveloperOptions existingOptions, string key, string value)
        {
            if ("accesskey".Equals(key))
            {
                existingOptions.AccessKey = value;
                return;
            }

            if ("secretkey".Equals(key))
            {
                existingOptions.SecretAccessKey = value;
                return;
            }

            if ("regionendpoint".Equals(key))
            {
                existingOptions.RegionEndpoint = value;
            }

            if ("availabilityzone".Equals(key))
            {
                existingOptions.AvailabilityZone = value;
                return;
            }

            if ("allocatedstorage".Equals(key))
            {
                int result;
                if (!int.TryParse(value, out result))
                {
                    throw new ArgumentException(string.Format("The developer option '{0}' can only have an integer value but '{1}' was used instead.", key, value));
                }
                existingOptions.AllocatedStorage = result;
                return;
            }

            if ("allowversionupgrade".Equals(key))
            {
                bool result;
                if (!bool.TryParse(value, out result))
                {
                    throw new ArgumentException(string.Format("The developer option '{0}' must be a boolean value.", key));
                }
                existingOptions.AllowVersionUpgrade = result;
                return;
            }

            if ("automatedsnapshotretentionperiod".Equals(key))
            {
                int result;
                if (!int.TryParse(value, out result))
                {
                    throw new ArgumentException(string.Format("The developer option '{0}' can only have an integer value but '{1}' was used instead.", key, value));
                }
                existingOptions.AutomatedSnapshotRetentionPeriod = result;
                return;
            }

            if ("clusteridentifier".Equals(key))
            {
                existingOptions.ClusterIdentifier = value;
                return;
            }

            if ("clusterparametergroupname".Equals(key))
            {
                existingOptions.ClusterParameterGroupName = value;
                return;
            }

            if ("clustersecuritygroups".Equals(key))
            {
                if (true)
                {
                    existingOptions.ClusterSecurityGroups.Add(value);
                }
                return;
            }

            if ("clustersubnetgroupname".Equals(key))
            {
                existingOptions.ClusterSubnetGroupName = value;
                return;
            }

            if ("clustertype".Equals(key))
            {
                existingOptions.ClusterType = value;
                return;
            }

            if ("clusterversion".Equals(key))
            {
                existingOptions.ClusterVersion = value;
                return;
            }

            if ("dbname".Equals(key))
            {
                existingOptions.DbName = value;
                return;
            }

            if ("elasticip".Equals(key))
            {
                existingOptions.ElasticIp = value;
                return;
            }

            if ("encrypted".Equals(key))
            {
                bool result;
                if (!bool.TryParse(value, out result))
                {
                    throw new ArgumentException(string.Format("The developer option '{0}' must be a boolean value.", key));
                }
                existingOptions.Encrypted = result;
                return;
            }

            if ("hsmclientcertificateidentifier".Equals(key))
            {
                existingOptions.HsmClientCertificateIdentifier = value;
                return;
            }

            if ("hsmclientconfigurationidentifier".Equals(key))
            {
                existingOptions.HsmClientConfigurationIdentifier = value;
                return;
            }

            if ("masterpassword".Equals(key))
            {
                existingOptions.MasterPassword = value;
                return;
            }

            if ("masterusername".Equals(key))
            {
                existingOptions.MasterUserName = value;
                return;
            }

            if ("nodetype".Equals(key))
            {
                existingOptions.NodeType = value;
                return;
            }

            if ("numberofnodes".Equals(key))
            {
                int result;
                if (!int.TryParse(value, out result))
                {
                    throw new ArgumentException(string.Format("The developer option '{0}' must be a boolean value.", key));
                }
                existingOptions.NumberOfNodes = result;
                return;
            }

            if ("port".Equals(key))
            {
                int result;
                if (!int.TryParse(value, out result))
                {
                    throw new ArgumentException(string.Format("The developer option '{0}' must be a boolean value.", key));
                }
                existingOptions.Port = result;
                return;
            }

            if ("preferredmaintenancewindow".Equals(key))
            {
                existingOptions.PreferredMaintenanceWindow = value;
                return;
            }

            if ("publiclyaccessible".Equals(key))
            {
                bool result;
                if (!bool.TryParse(value, out result))
                {
                    throw new ArgumentException(string.Format("The developer option '{0}' must be a boolean value.", key));
                }
                existingOptions.PubliclyAccessible = result;
                return;
            }

            if ("secretaccesskey".Equals(key))
            {
                existingOptions.SecretAccessKey = value;
                return;
            }

            if ("vpcsecuritygroupids".Equals(key))
            {
                existingOptions.VpcSecurityGroupIds.Add(value);
                return;
            }
            throw new ArgumentException(string.Format("The developer option '{0}' was not expected and is not understood.", key));
        }
    }
}