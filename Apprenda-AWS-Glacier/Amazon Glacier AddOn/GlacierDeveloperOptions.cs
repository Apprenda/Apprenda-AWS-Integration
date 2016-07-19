using System;
using System.Collections.Generic;

namespace Apprenda.SaaSGrid.Addons.AWS.Glacier
{
    public class GlacierDeveloperOptions
    {
        // Amazon Credentials. Required for IAM. 
        public string VaultName { get; private set; }
        public string AccountId { get; private set; }

        // Amazon RDS Options required for 

        // Method takes in a string and parses it into a DeveloperOptions class.
        public static GlacierDeveloperOptions Parse(IEnumerable<AddonParameter> _developerParameters, AddonManifest _manifest)
        {
            var options = new GlacierDeveloperOptions();
            foreach (var x in _manifest.Properties)
            {
                MapToOption(options, x.Key.ToLowerInvariant(), x.Value);
            }
            foreach (var parameter in _developerParameters)
            {
                MapToOption(options, parameter.Key.ToLowerInvariant(), parameter.Value);
            }
            return options;
        }

        // Interior method takes in instance of DeveloperOptions (aptly named existingOptions) and maps them to the proper value. In essence, a setter method.
        private static void MapToOption(GlacierDeveloperOptions _existingOptions, string _key, string _value)
        {
            if ("vaultname".Equals(_key))
            {
                _existingOptions.VaultName = _value;
                return;
            }
            if ("awsaccountid".Equals(_key))
            {
                _existingOptions.AccountId = _value;
                return;
            }
            throw new ArgumentException(string.Format("The developer option '{0}' was not expected and is not understood.", _key));
        }
    }
}