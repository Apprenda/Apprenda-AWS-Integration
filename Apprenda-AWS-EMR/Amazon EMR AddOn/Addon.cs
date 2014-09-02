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


namespace Apprenda.SaaSGrid.Addons.AWS.EMR
{
    public class AddOn : AddonBase
    {
        public override ProvisionAddOnResult Provision(AddonProvisionRequest request)
        {
            var provisionResult = new ProvisionAddOnResult("") { IsSuccess = true };
            AddonManifest manifest = request.Manifest;
            string developerOptions = request.DeveloperOptions;

            try
            {
                IAmazonElasticMapReduce client;
                EMRDeveloperOptions devOptions;

                var parseOptionsResult = ParseDevOptions(developerOptions, out devOptions);
                if (!parseOptionsResult.IsSuccess)
                {
                    provisionResult.EndUserMessage = parseOptionsResult.EndUserMessage;
                    return provisionResult;
                }

                var establishClientResult = EstablishClient(manifest, EMRDeveloperOptions.Parse(developerOptions), out client);
                if (!establishClientResult.IsSuccess)
                {
                    provisionResult.EndUserMessage = establishClientResult.EndUserMessage;
                    return provisionResult;
                }

                var stepFactory = new StepFactory();

                StepConfig enabledebugging = null;
                // if the devs request that debugging be enabled, we'll add the step here.
                if (devOptions.EnableDebugging)
                {
                    enabledebugging = new StepConfig
                    {
                        Name = "Enable debugging",
                        ActionOnFailure = "TERMINATE_JOB_FLOW",
                        HadoopJarStep = stepFactory.NewEnableDebuggingStep()
                    };
                }

                var installHive = new StepConfig
                {
                    Name = "Install Hive",
                    ActionOnFailure = "TERMINATE_JOB_FLOW",
                    HadoopJarStep = stepFactory.NewInstallHiveStep()
                };

                var instanceConfig = new JobFlowInstancesConfig
                {
                    Ec2KeyName = devOptions.Ec2KeyName,
                    HadoopVersion = "0.20",
                    InstanceCount = devOptions.InstanceCount,
                    // this is important. must be kept alive for the application to see it during provisioning
                    KeepJobFlowAliveWhenNoSteps = true,
                    MasterInstanceType = devOptions.MasterInstanceType,
                    SlaveInstanceType = devOptions.SlaveInstanceType
                };
            
                var _request = new RunJobFlowRequest
                {
                    Name = devOptions.JobFlowName,
                    Steps = { enabledebugging, installHive },
                    LogUri = "s3://myawsbucket",
                    Instances = instanceConfig
                };

                // if debugging is enabled, add to top of the list of steps.
                if(devOptions.EnableDebugging)
                {
                   _request.Steps.Insert(0, enabledebugging); 
                }

                var result = client.RunJobFlow(_request);

                // wait for JobFlowID to come back.
                while (result.JobFlowId == null)
                {
                    Thread.Sleep(1000);
                }

                provisionResult.IsSuccess = true;
                provisionResult.ConnectionData = string.Format(result.JobFlowId);
                
            }

            catch (Exception e)
            {
                provisionResult.EndUserMessage = e.Message;
            }

            return provisionResult;
        }

        public override OperationResult Deprovision(AddonDeprovisionRequest request)
        {
            var deprovisionResult = new ProvisionAddOnResult("") { IsSuccess = true };
            deprovisionResult.ConnectionData = "deprovision";
            AddonManifest manifest = request.Manifest;
            string connectionData = request.ConnectionData;
            string devOptions = request.DeveloperOptions;
            //string jobid = null;

            try
            {
                IAmazonElasticMapReduce client;
                //var conInfo = ConnectionInfo.Parse(connectionData);
                var developerOptions = EMRDeveloperOptions.Parse(devOptions);

                var establishClientResult = EstablishClient(manifest, developerOptions, out client);
                if (!establishClientResult.IsSuccess)
                {
                    deprovisionResult.EndUserMessage = establishClientResult.EndUserMessage;
                    return deprovisionResult;
                }

                var result = client.TerminateJobFlows(new TerminateJobFlowsRequest() { JobFlowIds = {connectionData}});
                
                deprovisionResult.IsSuccess = true;
                deprovisionResult.EndUserMessage = "EMR Cluster Termination Request Successfully Invoked.";
            }
            catch (Exception)
            {
                deprovisionResult.EndUserMessage = "An error occurred during deprovisioning, please check the SOC logs for further assistance.";
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
                EMRDeveloperOptions devOptions;
                
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

        private bool ValidateDevCreds(EMRDeveloperOptions devOptions)
        {
            return !(string.IsNullOrWhiteSpace(devOptions.AccessKey) || string.IsNullOrWhiteSpace(devOptions.SecretAccessKey));
        }

        private OperationResult ParseDevOptions(string developerOptions, out EMRDeveloperOptions devOptions)
        {
            devOptions = null;
            var result = new OperationResult() { IsSuccess = false };
            var progress = "";

            try
            {
                progress += "Parsing developer options...\n";
                devOptions = EMRDeveloperOptions.Parse(developerOptions);
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

        private OperationResult EstablishClient(AddonManifest manifest, EMRDeveloperOptions devOptions, out IAmazonElasticMapReduce client)
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
                secretAccessKey = devOptions.SecretAccessKey;

            }

            AWSCredentials credentials = new BasicAWSCredentials(accessKey, secretAccessKey);
            client = AWSClientFactory.CreateAmazonElasticMapReduceClient(credentials, RegionEndpoint.USEast1);


            //jobid = job.JobFlowId;

            result = new OperationResult()
            {
                IsSuccess = true
            };

            return result;
        }


        public string EndUserMessage { get; set; }
    }
}
