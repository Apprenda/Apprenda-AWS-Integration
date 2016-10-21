using System;
using System.Collections.Generic;

namespace Apprenda.SaaSGrid.Addons.AWS.S3
{
    using Amazon.S3;

    public class S3DeveloperOptions
    {
        public string BucketName { get; set; }
        public string RegionEndpoint { get; set; }
        public string BucketRegionName { get; set; }
        public string CannedAcl { get; set; }
        public bool UseClientRegion { get; set; }
        private string Grants { get; set; }

        private static void MapToOption(S3DeveloperOptions _existingOptions, string _key, string _value)
        {
            if ("bucketname".Equals(_key))
            {
                _existingOptions.BucketName = _value;
                return;
            }
            if ("bucketregionname".Equals(_key))
            {
                _existingOptions.BucketRegionName = _value;
                return;
            }
            if ("cannedacl".Equals(_key))
            {
                _existingOptions.CannedAcl = _value;
                return;
            }
            if ("useclientregion".Equals(_key))
            {
                bool result;
                if (!bool.TryParse(_value, out result))
                {
                    throw new Exception("Cannot parse boolean value.");
                }
                return;
            }
            if ("regionendpoint".Equals(_key))
            {
                _existingOptions.RegionEndpoint = _value;
                return;
            }
            if ("grants".Equals(_key))
            {
                _existingOptions.Grants = _value;
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
            if ("instancealias".Equals(_key))
            {
                return;
            }
            // else option is not found, throw exception
            throw new ArgumentException(string.Format("The developer option '{0}' was not expected and is not understood.", _key));
        }

        // This is the new method. give this a test!
        public static S3DeveloperOptions Parse(IEnumerable<AddonParameter> _developerParameters, AddonManifest manifest)
        {
            // TODO
            // given developerParameters, map the
            var options = new S3DeveloperOptions();
            foreach (var parameter in manifest.Properties)
            {
                MapToOption(options, parameter.Key.ToLowerInvariant(), parameter.Value);
            }
            foreach (var parameter in _developerParameters)
            {
                MapToOption(options, parameter.Key.ToLowerInvariant(), parameter.Value);
            }
            return options;
        }
    }
}