namespace Apprenda.SaaSGrid.Addons.AWS.RDS
{
    using System;
    using System.Collections.Generic;
    using Apprenda.SaaSGrid.Addons;

    public class RdsDeveloperOptions
    {
        // Amazon RDS Options required for
        public int AllocatedStorage { get; private set; }

        public bool AutoMinorVersionUpgrade { get; private set; }

        public string AvailabilityZone { get; private set; }

        public string DbInstanceClass { get; private set; }

        public string DbInstanceIdentifier { get; private set; }

        public string DbName { get; private set; }

        public string Engine { get; private set; }

        public string EngineVersion { get; private set; }

        public string DbaUsername { get; private set; }

        public string DbaPassword { get; private set; }

        public string LicenseModel { get; private set; }

        public int Port { get; set; }

        public int ProvisionedIoPs { get; set; }

        public List<string> DbSecurityGroups { get; set; }

        public string OptionGroup { get; set; }

        public string PreferredMxWindow { get; set; }

        public string PreferredBackupWindow { get; set; }

        private int NumberOfBackups { get; set; }

        public string SubnetGroupName { get; set; }

        public bool PubliclyAccessible { get; set; }

        public string CharacterSet { get; set; }

        public int BackupRetentionPeriod { get; set; }

        public bool SkipFinalSnapshot { get; set; }

        // Method takes in a string and parses it into a DeveloperOptions class.
        public static RdsDeveloperOptions Parse(IEnumerable<AddonParameter> _parameters, AddonManifest _manifest)
        {
            var options = new RdsDeveloperOptions();
            foreach (var manifestp in _manifest.Properties)
            {
                MapToOption(options, manifestp.Key.ToLowerInvariant(), manifestp.Value);
            }
            foreach (var addonparam in _parameters)
            {
                MapToOption(options, addonparam.Key.ToLowerInvariant(), addonparam.Value);
            }
            return options;
        }

        // Interior method takes in instance of DeveloperOptions (aptly named existingOptions) and maps them to the proper value. In essence, a setter method.
        private static void MapToOption(RdsDeveloperOptions _existingOptions, string _key, string _value)
        {
            if ("availabilityzone".Equals(_key))
            {
                _existingOptions.AvailabilityZone = _value;
                return;
            }

            if ("dbinstanceclass".Equals(_key))
            {
                _existingOptions.DbInstanceClass = _value;
                return;
            }

            if ("dbinstanceidentifier".Equals(_key))
            {
                _existingOptions.DbInstanceIdentifier = _value;
                return;
            }

            if ("dbname".Equals(_key))
            {
                _existingOptions.DbName = _value;
                return;
            }

            if ("engine".Equals(_key))
            {
                _existingOptions.Engine = _value;
                return;
            }

            if ("engineversion".Equals(_key))
            {
                _existingOptions.EngineVersion = _value;
            }

            if ("licensemodel".Equals(_key))
            {
                _existingOptions.LicenseModel = _value;
                return;
            }

            if ("dbausername".Equals(_key))
            {
                _existingOptions.DbaUsername = _value;
                return;
            }

            if ("dbapassword".Equals(_key))
            {
                _existingOptions.DbaPassword = _value;
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

            if ("autominorversionupgrade".Equals(_key))
            {
                bool result;
                if (!bool.TryParse(_value, out result))
                {
                    throw new ArgumentException(string.Format("The developer option '{0}' can only have a value of true|false but '{1}' was used instead.", _key, _value));
                }
                _existingOptions.AutoMinorVersionUpgrade = result;
                return;
            }

            if ("numberofbackups".Equals(_key))
            {
                int iresult;
                if (!int.TryParse(_value, out iresult))
                {
                    throw new ArgumentException(string.Format("The developer option '{0}' must be an integer, not '{1}'", _key, _value));
                }
                _existingOptions.NumberOfBackups = iresult;
                return;
            }
            
            if ("skipfinalsnapshot".Equals(_key))
            {
                bool result;
                if (!bool.TryParse(_value, out result))
                {
                    throw new ArgumentException(string.Format("The developer option '{0}' can only have a value of true|false but '{1}' was used instead.", _key, _value));
                }
                _existingOptions.SkipFinalSnapshot = result;
                return;
            }

            // default behavior - if nothing parses, then throw exception.
            throw new ArgumentException("The developer option '{0}' did not parse. Please check your configuration.", _key);
            
        }

    }
}