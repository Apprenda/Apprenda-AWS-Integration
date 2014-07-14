using System;
using System.Collections.Generic;

namespace Apprenda.SaaSGrid.Addons.AWS.RDS
{
    public class DeveloperOptions
    {
        public bool MultiAZ { get; set; }
        public List<Amazon.RDS.Model.Tag> Tags { get; set; }
        public List<string> VPCSecurityGroupIds { get; set; }
        public string DBParameterGroupName {get; set;}

        // Amazon Credentials. Required for IAM. 
        public string AccessKey { get; set; }
        public string SecretAccessKey { get; set; }

        // Amazon RDS Options required for 
        public int AllocatedStorage { get; set; }
        public bool AutoMinorVersionUpgrade { get; set; }
        public string AvailabilityZone { get; set; }
        public string DbInstanceClass { get; set; }
        public string DbInstanceIdentifier { get; set; }
        public string DbName { get; set; }
        public string Engine { get; set; }
        public string EngineVersion { get; set; }
        public string DBAUsername { get; set; }
        public string DBAPassword { get; set; }
        public string LicenseModel { get; set; }
        public int Port { get; set; }
        public int ProvisionedIOPs { get; set; }
        public List<String> DBSecurityGroups { get; set; }
        public String OptionGroup { get; set; }
        public String PreferredMXWindow { get; set; }
        public String PreferredBackupWindow { get; set; }
        public int NumberOfBackups { get; set; }
        public String SubnetGroupName { get; set; }
        public bool PubliclyAccessible { get; set; }
        public String CharacterSet { get; set; }
        public int BackupRetentionPeriod { get; set; }
        // Method takes in a string and parses it into a DeveloperOptions class.
        public static DeveloperOptions Parse(string developerOptions)
        {
            DeveloperOptions options = new DeveloperOptions();

            if (!string.IsNullOrWhiteSpace(developerOptions))
            {
                var optionPairs = developerOptions.Split(new []{'&'}, StringSplitOptions.RemoveEmptyEntries);
                foreach (var optionPair in optionPairs)
                {
                    var optionPairParts = optionPair.Split(new[]{'='}, StringSplitOptions.RemoveEmptyEntries);
                    if (optionPairParts.Length == 2)
                    {
                        MapToOption(options, optionPairParts[0].Trim().ToLowerInvariant(), optionPairParts[1].Trim());
                    }
                    else
                    {
                        throw new ArgumentException(
                            string.Format(
                                "Unable to parse developer options which should be in the form of 'option=value&nextOption=nextValue'. The option '{0}' was not properly constructed",
                                optionPair));
                    }
                }
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

            throw new ArgumentException(string.Format("The developer option '{0}' was not expected and is not understood.", key));
        }
    }
}