using System;
using System.Collections.Generic;

namespace Apprenda.SaaSGrid.Addons.AWS.RDS
{
    public class DeveloperOptions
    {
        public bool MultiAz { get; set; }

        public List<Amazon.RDS.Model.Tag> Tags { get; set; }

        public List<string> VpcSecurityGroupIds { get; set; }

        public string DbParameterGroupName { get; set; }

        // Amazon Credentials. Required for IAM.
        public string AccessKey { get; private set; }

        public string SecretAccessKey { get; private set; }

        // Amazon RDS Options required for
        public int AllocatedStorage { get; private set; }

        public bool AutoMinorVersionUpgrade { get; private set; }

        public string AvailabilityZone { get; private set; }

        public string DbInstanceClass { get; private set; }

        public string DbInstanceIdentifier { get; private set; }

        public string DbName { get; private set; }

        public string Engine { get; private set; }

        public string EngineVersion { get; private set; }

        public string DBAUsername { get; private set; }

        public string DBAPassword { get; private set; }

        public string LicenseModel { get; private set; }

        public int Port { get; set; }

        public int ProvisionedIOPs { get; set; }

        public List<String> DBSecurityGroups { get; set; }

        public String OptionGroup { get; set; }

        public String PreferredMXWindow { get; set; }

        public String PreferredBackupWindow { get; set; }

        private int NumberOfBackups { get; set; }

        public String SubnetGroupName { get; set; }

        public bool PubliclyAccessible { get; set; }

        public String CharacterSet { get; set; }

        public int BackupRetentionPeriod { get; set; }

        // Method takes in a string and parses it into a DeveloperOptions class.
        public static DeveloperOptions Parse(IEnumerable<AddonParameter> parameters)
        {
            var options = new DeveloperOptions();
            foreach (var addonparam in parameters)
            {
                MapToOption(options, addonparam.Key.ToLowerInvariant(), addonparam.Value);
            }
            return options;
        }

        // Interior method takes in instance of DeveloperOptions (aptly named existingOptions) and maps them to the proper value. In essence, a setter method.
        private static void MapToOption(DeveloperOptions existingOptions, string key, string value)
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

            if ("availabilityzone".Equals(key))
            {
                existingOptions.AvailabilityZone = value;
                return;
            }

            if ("dbinstanceclass".Equals(key))
            {
                existingOptions.DbInstanceClass = value;
                return;
            }

            if ("dbinstanceidentifier".Equals(key))
            {
                existingOptions.DbInstanceIdentifier = value;
                return;
            }

            if ("dbname".Equals(key))
            {
                existingOptions.DbName = value;
                return;
            }

            if ("engine".Equals(key))
            {
                existingOptions.Engine = value;
                return;
            }

            if ("engineversion".Equals(key))
            {
                existingOptions.EngineVersion = value;
            }

            if ("licensemodel".Equals(key))
            {
                existingOptions.LicenseModel = value;
                return;
            }

            if ("dbausername".Equals(key))
            {
                existingOptions.DBAUsername = value;
                return;
            }

            if ("dbapassword".Equals(key))
            {
                existingOptions.DBAPassword = value;
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

            if ("autominorversionupgrade".Equals(key))
            {
                bool result;
                if (!bool.TryParse(value, out result))
                {
                    throw new ArgumentException(string.Format("The developer option '{0}' can only have a value of true|false but '{1}' was used instead.", key, value));
                }
                existingOptions.AutoMinorVersionUpgrade = result;
                return;
            }

            if (!"numberofbackups".Equals(key))
                throw new ArgumentException(
                    string.Format("The developer option '{0}' was not expected and is not understood.", key));
            int iresult;
            if (!int.TryParse(value, out iresult))
            {
                throw new ArgumentException(string.Format("The developer option '{0}' must be an integer, not '{1}'", key, value));
            }
            existingOptions.NumberOfBackups = iresult;
            return;
        }
    }
}