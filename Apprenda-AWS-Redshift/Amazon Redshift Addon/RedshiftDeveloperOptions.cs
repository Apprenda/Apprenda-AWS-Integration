using System;
using System.Collections.Generic;

namespace Apprenda.SaaSGrid.Addons.AWS.Redshift
{
    using Apprenda.Services.Logging;
    public class RedshiftDeveloperOptions
    {
        private static readonly ILogger logger = LogManager.Instance().GetLogger(typeof(RedshiftDeveloperOptions));
        private const bool DefaultAllowVersionUpgrade = true;

        private const uint MaxAutomatedSnapshotPeriod = 35;

        private const uint DefaultAutomatedSnapshotPeriod = 1;

        private const string DefaultClusterType = "multi-node";

        private const bool DefaultEncrypted = false;

        private const uint DefaultNumberOfNodes = 1;

        private static readonly List<string> AllowedNodeTypes = new List<string>
        { "ds1.xlarge", "ds1.8xlarge", "ds2.xlarge","ds2.8xlarge", "dc1.large", "dc1.8xlarge"};

        private const uint DefaultPort = 5439;

        private const uint DefaultNodeCount = 1;

        public bool SkipFinalSnapshot { get; private set; }
        public string ClusterParameterGroupName { get; private set; }
        public string MasterUserName { get; private set; }
        public string MasterUserPassword { get; private set; }
        //public uint AllocatedStorage { get; private set; }
        //public uint MaxAllocatedStorage { get; private set; }
        //public string AvailabilityZone { get; private set; }
        public bool AllowVersionUpgrade { get; private set; }
        public uint AutomatedSnapshotRetentionPeriod { get; private set; }
        public string ClusterIdentifier { get; private set; }
        public List<string> ClusterSecurityGroups { get; set; }
        public string ClusterSubnetGroupName { get; private set; }
        public string ClusterType { get; private set; }
        //public string ClusterVersion { get; private set; }
        public string DbName { get; private set; }
        //public string ElasticIp { get; private set; }
        public bool Encrypted { get; private set; }
        public string HsmClientCertificateIdentifier { get; private set; }
        public string HsmClientConfigurationIdentifier { get; private set; }
        public string NodeType { get; private set; }
        public uint NumberOfNodes { get; private set; }
        public uint MaxNumberOfNodes { get; private set; }
        public uint Port { get; private set; }
        public string PreferredMaintenanceWindow { get; private set; }
        public bool PubliclyAccessible { get; private set; }
        public List<string> VpcSecurityGroupIds { get; set; }

        private RedshiftDeveloperOptions()
        {
            this.AllowVersionUpgrade = DefaultAllowVersionUpgrade;
            this.AutomatedSnapshotRetentionPeriod = DefaultAutomatedSnapshotPeriod;
            this.ClusterType = DefaultClusterType;
            this.Encrypted = DefaultEncrypted;
            this.NumberOfNodes = DefaultNumberOfNodes;
            this.Port = DefaultPort;
        }

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
                uint result;
                if (!uint.TryParse(_value, out result))
                {
                    logger.WarnFormat("The developer option '{0}' can only have an integer value but '{1}' was used instead. Using default.", _key, _value);
                    return;
                }
                if (result > MaxAutomatedSnapshotPeriod)
                {
                    throw new ArgumentOutOfRangeException("Automated Snapshot Retention period should be between 0 and 35.");
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
                _existingOptions.ClusterSecurityGroups.Add(_value);
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
            if ("dbname".Equals(_key))
            {
                _existingOptions.DbName = _value;
                return;
            }
            /*
             * Not concerned with VPC
            if ("elasticip".Equals(_key))
            {
                _existingOptions.ElasticIp = _value;
                return;
            }
            */
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

            if ("masteruserpassword".Equals(_key))
            {
                _existingOptions.MasterUserPassword = _value;
                return;
            }

            if ("masterusername".Equals(_key))
            {
                _existingOptions.MasterUserName = _value;
                return;
            }

            if ("nodetype".Equals(_key))
            {
                if (AllowedNodeTypes.Contains(_value))
                {
                    _existingOptions.NodeType = _value;
                }
                else throw new ArgumentException("Nodetype doesn't match list of allowed values.");
                return;
            }

            if ("nodecount".Equals(_key))
            {
                uint result;
                if (!uint.TryParse(_value, out result))
                {
                    logger.WarnFormat("The developer option '{0}' must be a numerical value. Reverting to default node count: {1}", _key, DefaultNodeCount);
                }
                _existingOptions.NumberOfNodes = result;
                return;
            }

            if ("maxnumberofnodes".Equals(_key))
            {
                uint result;
                if (!uint.TryParse(_value, out result))
                {
                    throw new ArgumentException(string.Format("The developer option '{0}' must be a numerical value.", _key));
                }
                _existingOptions.MaxNumberOfNodes = result;
                return;
            }

            if ("port".Equals(_key))
            {
                uint result;
                if (!uint.TryParse(_value, out result))
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
            if ("skipfinalsnapshot".Equals(_key))
            {
                bool result;
                if (!bool.TryParse(_value, out result))
                {
                    throw new ArgumentException(string.Format("The developer option '{0}' must be a boolean value.", _key));
                }
                _existingOptions.SkipFinalSnapshot = result;
                return;
            }
            if ("vpcsecuritygroupids".Equals(_key))
            {
                _existingOptions.VpcSecurityGroupIds.Add(_value);
                return;
            }
            if ("developerid".Equals(_key))
            {
                return;
            }
            if ("developeralias".Equals(_key))
            {
                return;
            }
            throw new ArgumentException(string.Format("The developer option '{0}' was not expected and is not understood.", _key));
        }
    }
}