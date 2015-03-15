using System.Collections.Generic;
using Amazon;
using Amazon.ElasticMapReduce;
using Amazon.ElasticMapReduce.Model;
using Amazon.Runtime;
using System;
using System.Linq;
using System.Threading;
using Apprenda.SaaSGrid.Addons.AWS.Util;

namespace Apprenda.SaaSGrid.Addons.AWS.EMR
{
    public class EmrAddOn : AddonBase
    {
        public override ProvisionAddOnResult Provision(AddonProvisionRequest request)
        {
            var provisionResult = new ProvisionAddOnResult("") { IsSuccess = true };
            var manifest = request.Manifest;
            var developerParameters = request.DeveloperParameters;

            try
            {
                IAmazonElasticMapReduce client;
                EMRDeveloperOptions devOptions;

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

                var stepFactory = new StepFactory();

                StepConfig enabledebugging = null;

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
                    // this is important. the EMR job flow must be kept alive for the application to see it during provisioning
                    KeepJobFlowAliveWhenNoSteps = true,
                    MasterInstanceType = devOptions.MasterInstanceType,
                    SlaveInstanceType = devOptions.SlaveInstanceType
                };

                var jobFlowRequestrequest = new RunJobFlowRequest
                {
                    Name = devOptions.JobFlowName,
                    Steps = { enabledebugging, installHive },
                    // revisit this one in ne
                    LogUri = "s3://myawsbucket",
                    Instances = instanceConfig
                };

                // if debugging is enabled, add to top of the list of steps.
                if (devOptions.EnableDebugging)
                {
                    jobFlowRequestrequest.Steps.Insert(0, enabledebugging);
                }

                var result = client.RunJobFlow(jobFlowRequestrequest);

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
            var manifest = request.Manifest;
            var connectionData = request.ConnectionData;
            var deprovisionResult = new OperationResult
            {
                IsSuccess = false
            };
            try
            {
                IAmazonElasticMapReduce client;

                var establishClientResult = EstablishClient(manifest, out client);
                if (!establishClientResult.IsSuccess)
                {
                    deprovisionResult.EndUserMessage = establishClientResult.EndUserMessage;
                    return deprovisionResult;
                }

                client.TerminateJobFlows(new TerminateJobFlowsRequest { JobFlowIds = { connectionData } });

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
            var developerParameters = request.DeveloperParameters;
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

        private OperationResult ParseDevOptions(IEnumerable<AddonParameter> developerParameters, out EMRDeveloperOptions devOptions)
        {
            try
            {
                devOptions = EMRDeveloperOptions.Parse(developerParameters);
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

        private OperationResult EstablishClient(IAddOnDefinition manifest, out IAmazonElasticMapReduce client)
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