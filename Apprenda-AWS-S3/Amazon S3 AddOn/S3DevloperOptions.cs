using System;
using System.Collections.Generic;

namespace Apprenda.SaaSGrid.Addons.AWS.S3
{
    public class S3DeveloperOptions
    {
        public S3DeveloperOptions(List<string> _grants=null)
        {
            this.Grants = _grants;
        }

        public string BucketName { get; set; }
        public string BucketRegion { get; set; }
        public string BucketRegionName { get; set; }
        public string CannedACL { get; set; }
        public bool UseClientRegion { get; set; }
        private List<string> Grants { get; set; }

        private static void MapToOption(S3DeveloperOptions _existingOptions, string _key, string _value)
        {
            if ("bucketname".Equals(_key))
            {
                _existingOptions.BucketName = _value;
                return;
            }
            if ("bucketregion".Equals(_key))
            {
                return;
            }
            if ("bucketregionname".Equals(_key))
            {
                return;
            }
            if ("cannedacl".Equals(_key))
            {
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
            if ("grants".Equals(_key))
            {
                _existingOptions.Grants.Add(_value);
                return;
            }
            // else option is not found, throw exception
            throw new ArgumentException(string.Format("The developer option '{0}' was not expected and is not understood.", _key));
        }

        // This is the new method. give this a test!
        public static S3DeveloperOptions Parse(IEnumerable<AddonParameter> _developerParameters)
        {
            // TODO
            // given developerParameters, map the
            var options = new S3DeveloperOptions();
            foreach (var parameter in _developerParameters)
            {
                MapToOption(options, parameter.Key.ToLowerInvariant(), parameter.Value);
            }
            return options;
        }
    }
}