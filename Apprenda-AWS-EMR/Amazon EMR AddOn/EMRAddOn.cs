using System.Collections.Generic;
using Amazon;
using Amazon.ElasticMapReduce;
using Amazon.Runtime;
using System;
using System.Linq;
using Apprenda.SaaSGrid.Addons.AWS.Util;

namespace Apprenda.SaaSGrid.Addons.AWS.EMR
{
    public class EmrAddOn : AddonBase
    {
        // ---------------------------------------------
        // we need to re-architect this. 
        // I strongly believe that an EMR solution is going to be decoupled from an application, and the Apprenda addon should manage the connection string to the EMR instance,
        // rather than create it. 
        // ---------------------------------------------
        // ---------------------------------------------
        // Provision - is going to provision
        // Assumptions - we already already have an EMR cluster, requirement here is to get a connection string to access it.
        // 
        // Supported formats, HIVE, Impala, and HBase
        // ---------------------------------------------
        public override ProvisionAddOnResult Provision(AddonProvisionRequest request)
        {
            var provisionResult = new ProvisionAddOnResult("") { IsSuccess = true };
            var manifest = request.Manifest;
            var developerParameters = request.DeveloperParameters;

            try
            {
                IAmazonElasticMapReduce client;
                EmrDeveloperOptions devOptions;

                var parseOptionsResult = ParseDevOptions(developerParameters, out devOptions);
                if (!parseOptionsResult.IsSuccess)
                {
                    provisionResult.EndUserMessage = parseOptionsResult.EndUserMessage;
                    return provisionResult;
                }

                var establishClientResult = EstablishClient(manifest, out client);
                if (!establishClientResult.IsSuccess)
                {
                    provisionResult.EndUserMessage = establishClientResult.EndUserMessage;
                    return provisionResult;
                }

                // ok, we need 
                // the EMR Cluster info from developer options
                // the protocol (Hive, Impala, HBase)

                var clusterResponse = client.DescribeCluster();
                // this is the endpoint you need for your client
                var dns = clusterResponse.Cluster.MasterPublicDnsName;
                var url = "";
                if (devOptions.Protocol.Equals(EmrProtocol.Jdbc))
                {
                    // and generate connection string!
                    if (devOptions.InterfaceType.Equals(EmrInterfaceType.HBase))
                    {
                        url = "jdbc:hbase://" + dns + ":" + devOptions.Port + "/default";
                    }
                    else if (devOptions.InterfaceType.Equals(EmrInterfaceType.Hive))
                    {
                        url = "jdbc:hbase://" + dns + ":" + devOptions.Port + "/default";
                    }
                    else if (devOptions.InterfaceType.Equals(EmrInterfaceType.Impala))
                    {
                        url = "jdbc:hbase://" + dns + ":" + devOptions.Port + "/default";
                    }
                }
                if (devOptions.Protocol.Equals(EmrProtocol.Odbc))
                {
                    // and generate connection string!
                    if (devOptions.InterfaceType.Equals(EmrInterfaceType.HBase))
                    {
                        url = "jdbc:hbql://" + dns + ":" + devOptions.Port + "/default";
                    }
                    else if (devOptions.InterfaceType.Equals(EmrInterfaceType.Hive))
                    {
                        url = "jdbc:hive://" + dns + ":" + devOptions.Port + "/default";
                    }
                    else if (devOptions.InterfaceType.Equals(EmrInterfaceType.Impala))
                    {
                        url = "jdbc:hive2://" + dns + ":" + devOptions.Port + "/default";
                    }
                }

                provisionResult.IsSuccess = true;
                provisionResult.ConnectionData = new EmrConnectionInfo
                {
                    EmrConnectionString = url
                }.ToString();
            }
            catch (Exception e)
            {
                provisionResult.EndUserMessage = e.Message;
            }

            return provisionResult;
        }

        // Deprovision - is going to delete the connector to EMR.
        public override OperationResult Deprovision(AddonDeprovisionRequest request)
        {
            // We aren't really doing much here. Just remove the connection string and we should be good.
            // -------------------------------------------------------------------------------------------
            return new OperationResult { EndUserMessage = "Deprovision complete.", IsSuccess =  true};
        }

        public override OperationResult Test(AddonTestRequest request)
        {
            var manifest = request.Manifest;
            var developerParameters = request.DeveloperParameters;
            var testResult = new OperationResult { IsSuccess = false };
            var testProgress = "";
            
            if (manifest.Properties != null && manifest.Properties.Any())
            {
                EmrDeveloperOptions devOptions;

                testProgress += "Evaluating required manifest properties...\n";
                if (!ValidateManifest(manifest, out testResult))
                {
                    return testResult;
                }

                var parseOptionsResult = ParseDevOptions(developerParameters, out devOptions);

                if (!parseOptionsResult.IsSuccess)
                {
                    return parseOptionsResult;
                }
                testProgress += parseOptionsResult.EndUserMessage;

                try
                {
                    testProgress += "Establishing connection to AWS...\n";
                    IAmazonElasticMapReduce client;

                    var establishClientResult = EstablishClient(manifest, out client);

                    if (!establishClientResult.IsSuccess)
                    {
                        return establishClientResult;
                    }
                    testProgress += establishClientResult.EndUserMessage;

                    testProgress += "Successfully passed all testing criteria!";
                    testResult.IsSuccess = true;
                    testResult.EndUserMessage = testProgress;
                }
                catch (Exception e)
                {
                    testResult.EndUserMessage = e.Message;
                }
            }
            else
            {
                testResult.EndUserMessage = "Missing required manifest properties (requireDevCredentials)";
            }

            return testResult;
        }

        private static bool ValidateManifest(AddonManifest manifest, out OperationResult testResult)
        {
            testResult = new OperationResult();

            var prop =
                    manifest.Properties.FirstOrDefault(
                        p => p.Key.Equals("requireDevCredentials", StringComparison.InvariantCultureIgnoreCase));

            if (prop == null || !prop.HasValue)
            {
                testResult.IsSuccess = false;
                testResult.EndUserMessage = "Missing required property 'requireDevCredentials'. This property needs to be provided as part of the manifest";
                return false;
            }

            if (string.IsNullOrWhiteSpace(manifest.ProvisioningUsername) ||
                string.IsNullOrWhiteSpace(manifest.ProvisioningPassword))
            {
                testResult.IsSuccess = false;
                testResult.EndUserMessage = "Missing credentials 'provisioningUsername' & 'provisioningPassword' . These values needs to be provided as part of the manifest";
                return false;
            }

            return true;
        }

        private static OperationResult ParseDevOptions(IEnumerable<AddonParameter> developerParameters, out EmrDeveloperOptions devOptions)
        {
            try
            {
                devOptions = EmrDeveloperOptions.Parse(developerParameters);
                return new OperationResult {IsSuccess = true};
            }
            catch (ArgumentException e)
            {
                devOptions = null;
                return new OperationResult
                {
                    IsSuccess = false,
                    EndUserMessage = e.Message
                };
            }
        }

        private static OperationResult EstablishClient(IAddOnDefinition manifest, out IAmazonElasticMapReduce client)
        {
            var accessKey = manifest.ProvisioningUsername;
            var secretAccessKey = manifest.ProvisioningPassword;
            var regionEndpoint = AWSUtils.ParseRegionEndpoint(manifest.ProvisioningLocation);
            AWSCredentials credentials = new BasicAWSCredentials(accessKey, secretAccessKey);
            client = AWSClientFactory.CreateAmazonElasticMapReduceClient(credentials, regionEndpoint);
            var result = new OperationResult
            {
                IsSuccess = true
            };

            return result;
        }
    }
}