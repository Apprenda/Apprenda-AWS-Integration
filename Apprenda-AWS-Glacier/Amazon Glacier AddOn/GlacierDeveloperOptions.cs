using System;
using System.Collections.Generic;

namespace Apprenda.SaaSGrid.Addons.AWS.Glacier
{
    public class GlacierDeveloperOptions
    {
        // Amazon Credentials. Required for IAM. 
        public string VaultName { get; private set; }

        // Amazon RDS Options required for 

        // Method takes in a string and parses it into a DeveloperOptions class.
        public static GlacierDeveloperOptions Parse(IEnumerable<AddonParameter> developerParameters)
        {
            var options = new GlacierDeveloperOptions();

            foreach (var parameter in developerParameters)
            {
                MapToOption(options, parameter.Key.ToLowerInvariant(), parameter.Value);
            }
            return options;
        }

        // Interior method takes in instance of DeveloperOptions (aptly named existingOptions) and maps them to the proper value. In essence, a setter method.
        private static void MapToOption(GlacierDeveloperOptions existingOptions, string key, string value)
        {
            if ("vaultname".Equals(key))
            {
                existingOptions.VaultName = value;
                return;
            }
            throw new ArgumentException(string.Format("The developer option '{0}' was not expected and is not understood.", key));
        }
    }
}