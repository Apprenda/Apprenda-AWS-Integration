using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.DataPipeline;
using Amazon.DataPipeline.Model;
using Amazon;
using Amazon.Runtime;
using System.Net;
using Apprenda.SaaSGrid.Addons;

namespace AWS_DataPipeLine_AddOn
{
    class PipelineOptions
    {
        public string AccessKey { get; set; }
        public string SecretAccessKey { get; set; }
        public string PipelineName { get; set; }
        public string PipelineDesc { get; set; }
        public string authenticationRegion { get; set; }

        public string authenticationServiceName { get; set; }

        public int buffersize { get; set; }

        public int connectionLimit { get; set; }

        public bool logmetrics { get; set; }

        public bool logresponse { get; set; }

        public int maxerrorretry { get; set; }

        public int maxidletime { get; set; }

        public long progressupdateinterval { get; set; }

        public ICredentials proxycredentials { get; set; }

        public string proxyhost { get; set; }

        public int proxyport { get; set; }

        public bool readentirerespone { get; set; }

        public TimeSpan? readwritetimeout { get; set; }

        public RegionEndpoint regionendpoint { get; set; }

        public string serviceurl { get; set; }

        public SigningAlgorithm signaturemethod { get; set; }

        public string signatureversion { get; set; }

        public TimeSpan? timeout { get; set; }

        public bool usehttp { get; set; }

        public bool usenaglealgorithm { get; set; }

        public string useragent { get; set; }

        public static PipelineOptions Parse(string developerOptions, AddonManifest manifest)
        {
            PipelineOptions options = new PipelineOptions();

            if (!string.IsNullOrWhiteSpace(developerOptions))
            {
                var optionPairs = developerOptions.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
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

            // Interior method takes in instance of DeveloperOptions (aptly named existingOptions) and maps them to the proper value. In essence, a setter method.
        private static void MapToOption(PipelineOptions existingOptions, string key, string value)
        {
            
            throw new ArgumentException(string.Format("The developer option '{0}' was not expected and is not understood.", key));
        }
         
    }
}
