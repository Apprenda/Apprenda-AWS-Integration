namespace Apprenda.SaaSGrid.Addons.AWS.RDS
{
    using System;
    using System.Collections.Generic;
    using Addons;

    public class RdsDeveloperOptions
    {
        private const bool DefaultAutoMinorVersionUpgrade = false;

        private const int DefaultBackupRetentionPeriod = 1;

        private const bool DefaultPubliclyAccessible = false;
        
        // Amazon RDS Options required for
        public int AllocatedStorage { get; private set; }
        public int MaxAllocatedStorage { get; private set; }
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
        public string OracleEngineEdition { get; set; }
        public string OracleDBVersion { get; set; }
        public string SqlServerEngineEdition { get; set; }
        public string SqlServerDBVersion { get; set; }
        public string MySqlEngineEdition { get; set; }
        public string MySqlDBVersion { get; set; }
        public bool MultiAz { get; set; }

        private RdsDeveloperOptions()
        {
            this.AutoMinorVersionUpgrade = DefaultAutoMinorVersionUpgrade;
            this.BackupRetentionPeriod = DefaultBackupRetentionPeriod;
            this.PubliclyAccessible = DefaultPubliclyAccessible;
        }

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

            //if ("dbinstanceclass".Equals(_key))
            //{
            //    _existingOptions.DbInstanceClass = _value;
            //    return;
            //}

            if ("maxdbinstanceclass".Equals(_key))
            {
                _existingOptions.DbInstanceClass = _value;
                return;
            }

            if ("dbinstanceidentifier".Equals(_key))
            {
                _existingOptions.DbInstanceIdentifier = _value;
                _existingOptions.DbName = _value;
                return;
            }

            //if ("dbname".Equals(_key))
            //{
            //    _existingOptions.DbName = _value;
            //    return;
            //}

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
            //if ("port".Equals(_key))
            //{
            //    int result;
            //    if (!int.TryParse(_value, out result))
            //    {
            //        throw new ArgumentException(string.Format("The developer option '{0}' can only have an integer value but '{1}' was used instead.", _key, _value));
            //    }
            //    _existingOptions.Port = result;
            //    return;
            //}
            if ("backupretentionperiod".Equals(_key))
            {
                int result;
                if (!int.TryParse(_value, out result))
                {
                    throw new ArgumentException(string.Format("The developer option '{0}' can only have an integer value but '{1}' was used instead.", _key, _value));
                }
                _existingOptions.BackupRetentionPeriod = result;
                return;
            }
            if ("storage".Equals(_key))
            {
                int result;
                if (!int.TryParse(_value, out result))
                {
                    throw new ArgumentException(string.Format("The developer option '{0}' can only have an integer value but '{1}' was used instead.", _key, _value));
                }
                _existingOptions.AllocatedStorage = result;
                if (_existingOptions.MaxAllocatedStorage < _existingOptions.AllocatedStorage)
                {
                    throw new ArgumentException(string.Format("The developer-defined storage size ({0} GB) exceeds the maximum storage ({1} GB) set by the Add-On", _existingOptions.AllocatedStorage, _existingOptions.MaxAllocatedStorage));
                }

                return;
            }

            if ("maxallocatedstorage".Equals(_key))
            {
                int result;
                if (!int.TryParse(_value, out result))
                {
                    throw new ArgumentException(string.Format("The developer option '{0}' can only have an integer value but '{1}' was used instead.", _key, _value));
                }
                _existingOptions.MaxAllocatedStorage = result;
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

            if ("oracleengineedition".Equals(_key))
            {
                _existingOptions.OracleEngineEdition = _value;
                return;
            }
            if ("oracledbversion".Equals(_key))
            {
                _existingOptions.OracleDBVersion = _value;
                return;
            }
            if ("sqlserverengineedition".Equals(_key))
            {
                _existingOptions.SqlServerEngineEdition = _value;
                return;
            }
            if ("sqlserverdbversion".Equals(_key))
            {
                _existingOptions.SqlServerDBVersion = _value;
                return;
            }
            if ("mysqlengineedition".Equals(_key))
            {
                _existingOptions.MySqlEngineEdition = _value;
                return;
            }
            if ("mysqldbversion".Equals(_key))
            {
                _existingOptions.MySqlDBVersion = _value;
                return;
            }
            if ("multiaz".Equals(_key))
            {
                bool result;
                if (!bool.TryParse(_value, out result))
                {
                    throw new ArgumentException(string.Format("The developer option '{0}' can only have a value of true|false but '{1}' was used instead.", _key, _value));
                }
                _existingOptions.MultiAz = result;
                return;
            }
            if ("publiclyaccessible".Equals(_key))
            {
                bool result;
                if (!bool.TryParse(_value, out result))
                {
                    throw new ArgumentException(string.Format("The developer option '{0}' can only have a value of true|false but '{1}' was used instead.", _key, _value));
                }
                _existingOptions.PubliclyAccessible = result;
                return;
            }
            if ("defaultaz".Equals(_key))
            {
                _existingOptions.AvailabilityZone = _value;
                return;
            }
            //if ("databasename".Equals(_key))
            //{
            //    _existingOptions.DbName = _value;
            //    return;
            //}
            if ("developerid".Equals(_key))
            {
                return;
            }
            if ("developeralias".Equals(_key))
            {
                return;
            }
            if ("instancealias".Equals(_key))
            {
                return;
            }
            if ("overrideport".Equals(_key))
            {
                // TBD: identify how to property set ports for each DB server options
                return;
            }

            // default behavior - if nothing parses, then throw exception.
            throw new ArgumentException(string.Format("The developer option '{0}' did not parse. Please check your configuration.", _key));            
        }

    }
}