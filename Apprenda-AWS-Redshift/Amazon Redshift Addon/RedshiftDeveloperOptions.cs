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

        public static RedshiftDeveloperOptions Parse(IEnumerable<AddonParameter> developerParameters, AddonManifest manifest)
        {
            // we made this so much easier with developer parameters.

            var options = new RedshiftDeveloperOptions();
            foreach (var parameter in manifest.Properties)
            {
                MapToOption(options, parameter.Key.ToLowerInvariant(), parameter.Value);
            }
            foreach (var parameter in developerParameters)
            {
                MapToOption(options, parameter.Key.ToLowerInvariant(), parameter.Value);
            }
            return options;
        }

        // Interior method takes in instance of DeveloperOptions (aptly named existingOptions) and maps them to the proper value. In essence, a setter method.
        private static void MapToOption(RedshiftDeveloperOptions _existingOptions, string _key, string _value)
        {
            if ("availabilityzone".Equals(_key))
            {
                _existingOptions.AvailabilityZone = _value;
                return;
            }

            if ("allocatedstorage".Equals(_key))
            {
                int result;
                if (!int.TryParse(_value, out result))
                {
                    throw new ArgumentException(string.Format("The developer option '{0}' can only have an integer value but '{1}' was used instead.", _key, _value));
                }
                _existingOptions.AllocatedStorage = result;
                return;
            }

            if ("allowversionupgrade".Equals(_key))
            {
                bool result;
                if (!bool.TryParse(_value, out result))
                {
                    throw new ArgumentException(string.Format("The developer option '{0}' must be a boolean value.", _key));
                }
                _existingOptions.AllowVersionUpgrade = result;
                return;
            }

            if ("automatedsnapshotretentionperiod".Equals(_key))
            {
                int result;
                if (!int.TryParse(_value, out result))
                {
                    throw new ArgumentException(string.Format("The developer option '{0}' can only have an integer value but '{1}' was used instead.", _key, _value));
                }
                _existingOptions.AutomatedSnapshotRetentionPeriod = result;
                return;
            }

            if ("clusteridentifier".Equals(_key))
            {
                _existingOptions.ClusterIdentifier = _value;
                return;
            }

            if ("clusterparametergroupname".Equals(_key))
            {
                _existingOptions.ClusterParameterGroupName = _value;
                return;
            }

            if ("clustersecuritygroups".Equals(_key))
            {
                if (true)
                {
                    _existingOptions.ClusterSecurityGroups.Add(_value);
                }
                return;
            }

            if ("clustersubnetgroupname".Equals(_key))
            {
                _existingOptions.ClusterSubnetGroupName = _value;
                return;
            }

            if ("clustertype".Equals(_key))
            {
                _existingOptions.ClusterType = _value;
                return;
            }

            if ("clusterversion".Equals(_key))
            {
                _existingOptions.ClusterVersion = _value;
                return;
            }

            if ("dbname".Equals(_key))
            {
                _existingOptions.DbName = _value;
                return;
            }

            if ("elasticip".Equals(_key))
            {
                _existingOptions.ElasticIp = _value;
                return;
            }

            if ("encrypted".Equals(_key))
            {
                bool result;
                if (!bool.TryParse(_value, out result))
                {
                    throw new ArgumentException(string.Format("The developer option '{0}' must be a boolean value.", _key));
                }
                _existingOptions.Encrypted = result;
                return;
            }

            if ("hsmclientcertificateidentifier".Equals(_key))
            {
                _existingOptions.HsmClientCertificateIdentifier = _value;
                return;
            }

            if ("hsmclientconfigurationidentifier".Equals(_key))
            {
                _existingOptions.HsmClientConfigurationIdentifier = _value;
                return;
            }

            if ("masterpassword".Equals(_key))
            {
                _existingOptions.MasterPassword = _value;
                return;
            }

            if ("masterusername".Equals(_key))
            {
                _existingOptions.MasterUserName = _value;
                return;
            }

            if ("nodetype".Equals(_key))
            {
                _existingOptions.NodeType = _value;
                return;
            }

            if ("numberofnodes".Equals(_key))
            {
                int result;
                if (!int.TryParse(_value, out result))
                {
                    throw new ArgumentException(string.Format("The developer option '{0}' must be a boolean value.", _key));
                }
                _existingOptions.NumberOfNodes = result;
                return;
            }

            if ("port".Equals(_key))
            {
                int result;
                if (!int.TryParse(_value, out result))
                {
                    throw new ArgumentException(string.Format("The developer option '{0}' must be a boolean value.", _key));
                }
                _existingOptions.Port = result;
                return;
            }

            if ("preferredmaintenancewindow".Equals(_key))
            {
                _existingOptions.PreferredMaintenanceWindow = _value;
                return;
            }

            if ("publiclyaccessible".Equals(_key))
            {
                bool result;
                if (!bool.TryParse(_value, out result))
                {
                    throw new ArgumentException(string.Format("The developer option '{0}' must be a boolean value.", _key));
                }
                _existingOptions.PubliclyAccessible = result;
                return;
            }

            if ("secretaccesskey".Equals(_key))
            {
                _existingOptions.SecretAccessKey = _value;
                return;
            }

            if ("vpcsecuritygroupids".Equals(_key))
            {
                _existingOptions.VpcSecurityGroupIds.Add(_value);
                return;
            }
            throw new ArgumentException(string.Format("The developer option '{0}' was not expected and is not understood.", _key));
        }
    }
}