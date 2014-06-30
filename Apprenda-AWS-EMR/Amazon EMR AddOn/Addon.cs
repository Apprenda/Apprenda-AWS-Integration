/*******************************************************************************
* Copyright 2009-2013 Amazon.com, Inc. or its affiliates. All Rights Reserved.
* 
* Licensed under the Apache License, Version 2.0 (the "License"). You may
* not use this file except in compliance with the License. A copy of the
* License is located at
* 
* http://aws.amazon.com/apache2.0/
* 
* or in the "license" file accompanying this file. This file is
* distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
* KIND, either express or implied. See the License for the specific
* language governing permissions and limitations under the License.
*******************************************************************************/

using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using Apprenda.SaaSGrid.Addons;
using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.EC2.Util;
using Amazon.IdentityManagement;
using Amazon.IdentityManagement.Model;
using Amazon.Auth.AccessControlPolicy;
using Amazon.Auth.AccessControlPolicy.ActionIdentifiers;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Amazon.ElasticMapReduce.Model;
using Amazon.ElasticMapReduce;
using Amazon.Runtime;


namespace AWS_EMR_AddOn
{
    public class AddOn : AddonBase
    {
        public override ProvisionAddOnResult Provision(AddonProvisionRequest request)
        {
            var provisionResult = new ProvisionAddOnResult() { IsSuccess = true };
            AddonManifest manifest = request.Manifest;
            string developerOptions = request.DeveloperOptions;

            try
            {
                IAmazonElasticMapReduce client;
                DeveloperOptions devOptions;

                var parseOptionsResult = ParseDevOptions(developerOptions, out devOptions);
                if (!parseOptionsResult.IsSuccess)
                {
                    provisionResult.EndUserMessage = parseOptionsResult.EndUserMessage;
                    return provisionResult;
                }

                var establishClientResult = EstablishClient(manifest, DeveloperOptions.Parse(developerOptions), out client);
                if (!establishClientResult.IsSuccess)
                {
                    provisionResult.EndUserMessage = establishClientResult.EndUserMessage;
                    return provisionResult;
                }
 
                provisionResult.IsSuccess = true;
                provisionResult.ConnectionData = string.Format("AWS AccessKey={0}; AWS SecretKey={1}", devOptions.AccessKey, devOptions.SecretKey);
                
            }

            catch (Exception)
            {
                provisionResult.EndUserMessage = "Placeholder for Provision";
            }

            return provisionResult;
        }

        public override OperationResult Deprovision(AddonDeprovisionRequest request)
        {
            var deprovisionResult = new ProvisionAddOnResult() { IsSuccess = true };
            deprovisionResult.ConnectionData = "deprovision";
            AddonManifest manifest = request.Manifest;
            string connectionData = request.ConnectionData;
            string devOptions = request.DeveloperOptions;
            //string jobid = null;

            try
            {
                IAmazonElasticMapReduce client;
                //var conInfo = ConnectionInfo.Parse(connectionData);
                var developerOptions = DeveloperOptions.Parse(devOptions);

                var establishClientResult = EstablishClient(manifest, developerOptions, out client);
                if (!establishClientResult.IsSuccess)
                {
                    deprovisionResult.EndUserMessage = establishClientResult.EndUserMessage;
                    return deprovisionResult;
                }
            }
            catch (Exception)
            {
                deprovisionResult.EndUserMessage = "Placeholder for Derovision";
            }

            return deprovisionResult;
        }

        public override OperationResult Test(AddonTestRequest request)
        {
            
            AddonManifest manifest = request.Manifest;
            string developerOptions = request.DeveloperOptions;
            var testResult = new OperationResult { IsSuccess = false };
            var testProgress = "";
            //string jobid = null;
            
            if (manifest.Properties != null && manifest.Properties.Any())
            {
                DeveloperOptions devOptions;
                
                testProgress += "Evaluating required manifest properties...\n";
                if (!ValidateManifest(manifest, out testResult))
                {
                   
                    return testResult;
                }

                var parseOptionsResult = ParseDevOptions(developerOptions, out devOptions);
                
                if (!parseOptionsResult.IsSuccess)
                {
                    
                    return parseOptionsResult;
                }
                testProgress += parseOptionsResult.EndUserMessage;
                
                try
                {
                    testProgress += "Establishing connection to AWS...\n";
                    IAmazonElasticMapReduce client;
                    
                    var establishClientResult = EstablishClient(manifest, devOptions, out client);
                    
                    if (!establishClientResult.IsSuccess)
                    {
                        
                        return establishClientResult;
                    }
                    testProgress += establishClientResult.EndUserMessage;
                    
                    testProgress += "Successfully passed all testing criteria!";
                    testResult.IsSuccess = true;
                    testResult.EndUserMessage = testProgress;
                }
                catch (Exception)
                {
                    testResult.EndUserMessage = "Placeholder for Test";
                }
            }
            else
            {
                testResult.EndUserMessage = "Missing required manifest properties (requireDevCredentials)";
            }

            return testResult;
        }

        private bool ValidateManifest(AddonManifest manifest, out OperationResult testResult)
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

        private bool ValidateDevCreds(DeveloperOptions devOptions)
        {
            return !(string.IsNullOrWhiteSpace(devOptions.AccessKey) || string.IsNullOrWhiteSpace(devOptions.SecretKey));
        }

        private OperationResult ParseDevOptions(string developerOptions, out DeveloperOptions devOptions)
        {
            devOptions = null;
            var result = new OperationResult() { IsSuccess = false };
            var progress = "";

            try
            {
                progress += "Parsing developer options...\n";
                devOptions = DeveloperOptions.Parse(developerOptions);
            }
            catch (ArgumentException)
            {
                result.EndUserMessage = "Placeholder for ValidateDevCreds";
                return result;
            }

            result.IsSuccess = true;
            result.EndUserMessage = progress;
            return result;
        }

        private OperationResult EstablishClient(AddonManifest manifest, DeveloperOptions devOptions, out IAmazonElasticMapReduce client)
        {
            OperationResult result;

            bool requireCreds;
            var accessKey = manifest.ProvisioningUsername;
            var secretAccessKey = manifest.ProvisioningPassword;
            var prop = manifest.Properties.First(p => p.Key.Equals("requireDevCredentials", StringComparison.InvariantCultureIgnoreCase));
            //jobid = null;

            if (bool.TryParse(prop.Value, out requireCreds) && requireCreds)
            {
                if (!ValidateDevCreds(devOptions))
                {
                    client = null;
                    result = new OperationResult()
                    {
                        IsSuccess = false,
                        EndUserMessage = "The add on requires that developer credentials are specified but none were provided."
                    };
                    
                    return result;
                }

                accessKey = devOptions.AccessKey;
                secretAccessKey = devOptions.SecretKey;

            }

            AWSCredentials credentials = new BasicAWSCredentials(accessKey, secretAccessKey);
            client = AWSClientFactory.CreateAmazonElasticMapReduceClient(credentials, RegionEndpoint.USEast1);
            
            
            var stepFactory = new StepFactory();

            var enabledebugging = new StepConfig
            {
                Name = "Enable debugging",
                ActionOnFailure = "TERMINATE_CLUSTER",
                HadoopJarStep = stepFactory.NewEnableDebuggingStep()
            };


            var instanceConfig = new JobFlowInstancesConfig
            {
                Ec2KeyName = "cs-keypair",
                HadoopVersion = "1.0.3",
                InstanceCount = 2,
                KeepJobFlowAliveWhenNoSteps = true,
                MasterInstanceType = "m1.small",
                SlaveInstanceType = "m1.small"
            };

            var runJobFlow = new RunJobFlowRequest
            {
                Name = "Startup EMR Cluster",
                Steps = { enabledebugging },
                LogUri = "s3://apprenda2/logs2/",
                Instances = instanceConfig
            };

            var job = client.RunJobFlow(runJobFlow);

            //jobid = job.JobFlowId;

            result = new OperationResult
            {
                IsSuccess = true,
            };

            return result;
        }


        public string EndUserMessage { get; set; }
    }
}
