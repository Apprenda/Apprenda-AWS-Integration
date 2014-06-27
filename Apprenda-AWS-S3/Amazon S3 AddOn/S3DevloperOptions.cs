using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon_Base_Addon;

namespace Amazon_S3_AddOn
{
    public class S3DeveloperOptions : DeveloperOptions
    {
        public String BucketName { get; set; }
        public String BucketRegion { get; set; }
        public String BucketRegionName { get; set; }
        public String CannedACL { get; set; }
        public List<String> Grants { get; set; }
        public bool UseClientRegion { get; set; }


        public static S3DeveloperOptions Parse(string devOptions)
        {
            S3DeveloperOptions options = new S3DeveloperOptions();

            if (!string.IsNullOrWhiteSpace(devOptions))
            {
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
            }
            return options;
        }

        // TODO
        public static void MapToOptionWithCollection(S3DeveloperOptions existingOptions, string key, string value, string lastKey)
        {
                if(key.Equals(lastKey))
                {
                   if ("grants".Equals(key))
                    {
                        existingOptions.Grants.Add(value);
                        return;
                    }
                    throw new ArgumentException(string.Format("The developer option '{0}' was not expected and is not understood.", key));
                }
                throw new ArgumentException(string.Format("The developer option '{0}' is grouped out of order in the REST call. Group collection parameters together in the request.", key));
        }


        public static void MapToOption(S3DeveloperOptions existingOptions, string key, string value)
        {
            if("bucketname".Equals(key))
            {
                existingOptions.BucketName = value;
                return;
            }
            if("bucketregion".Equals(key))
            {
                existingOptions.BucketRegion = value;
                return;
            }
            if("bucketregionname".Equals(key))
            {
                existingOptions.BucketRegionName = value;
                return;
            }
            if("cannedacl".Equals(key))
            {
                existingOptions.CannedACL = value;
                return;
            }
            if("useclientregion".Equals(key))
            {
                bool result;
                if(!bool.TryParse(value, out result))
                {
                    throw new Exception("Cannot parse boolean value.");
                }
                existingOptions.UseClientRegion = result;
                return;
            }
            if("grants".Equals(key))
            {
                existingOptions.Grants.Add(value);
                return;
            }
            // else option is not found, throw exception
            throw new ArgumentException(string.Format("The developer option '{0}' was not expected and is not understood.", key));
        }

    }
}
