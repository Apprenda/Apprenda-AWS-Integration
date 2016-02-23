using System;
using System.Collections.Generic;

namespace Apprenda.SaaSGrid.Addons.AWS.S3
{
    public class S3DeveloperOptions
    {
        internal String BucketName { get; set; }

        internal String BucketRegion { get; set; }

        internal String BucketRegionName { get; set; }

        internal String CannedAcl { get; set; }

        internal List<String> Grants { get; set; }

        internal bool UseClientRegion { get; set; }

        internal String RegionEndpont { get; set; }

        public string AccessKey { get; set; }

        public string SecretAccessKey { get; set; }

        [Obsolete]
        public static S3DeveloperOptions Parse(string devOptions)
        {
            var options = new S3DeveloperOptions();

            if (string.IsNullOrWhiteSpace(devOptions)) return options;
            var optionPairs = devOptions.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var optionPair in optionPairs)
            {
                var optionPairParts = optionPair.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
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
            return options;
        }

        private static void MapToOption(S3DeveloperOptions existingOptions, string key, string value)
        {
            if ("bucketname".Equals(key))
            {
                existingOptions.BucketName = value;
                return;
            }
            if ("bucketregion".Equals(key))
            {
                existingOptions.BucketRegion = value;
                return;
            }
            if ("bucketregionname".Equals(key))
            {
                existingOptions.BucketRegionName = value;
                return;
            }
            if ("cannedacl".Equals(key))
            {
                existingOptions.CannedAcl = value;
                return;
            }
            if ("useclientregion".Equals(key))
            {
                bool result;
                if (!bool.TryParse(value, out result))
                {
                    throw new Exception("Cannot parse boolean value.");
                }
                existingOptions.UseClientRegion = result;
                return;
            }
            if ("grants".Equals(key))
            {
                existingOptions.Grants.Add(value);
                return;
            }
            if ("regionendpoint".Equals(key))
            {
                existingOptions.RegionEndpont = value;
            }
            // else option is not found, throw exception
            throw new ArgumentException(string.Format("The developer option '{0}' was not expected and is not understood.", key));
        }

        // This is the new method. give this a test!
        public static S3DeveloperOptions ParseWithParameters(IEnumerable<AddonParameter> developerParameters)
        {
            // TODO
            // given developerParameters, map the
            var options = new S3DeveloperOptions();
            foreach (var parameter in developerParameters)
            {
                MapToOption(options, parameter.Key.ToLowerInvariant(), parameter.Value);
            }
            return options;
        }
    }
}