using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon_Base_Addon;
using Apprenda.SaaSGrid.Addons;
using Amazon.ElasticMapReduce.Model;
using Amazon.ElasticMapReduce;

namespace Amazon_EMR_AddOn
{
    public class EMRAddon : Addon
    {
        // Deprovision EMR JobFlow - this will destroy an EMR Job Flow(s)
        // Input: AddonDeprovisionRequest request
        // Output: OperationResult
        public override OperationResult Deprovision(AddonDeprovisionRequest request)
        {
            string connectionData = request.ConnectionData;
            var deprovisionResult = new ProvisionAddOnResult(connectionData);
            AddonManifest manifest = request.Manifest;

            string devOptions = request.DeveloperOptions;

            try
            {
                AmazonElasticMapReduceClient client;
                var conInfo = ConnectionInfo.Parse(connectionData);
                var developerOptions = EMRDeveloperOptions.Parse(devOptions);

                var establishClientResult = EstablishClient(manifest, developerOptions, out client);
                if (!establishClientResult.IsSuccess)
                {
                    deprovisionResult.EndUserMessage = establishClientResult.EndUserMessage;
                    return deprovisionResult;
                }

                // here, remove the job flow
                TerminateJobFlowsResponse response = client.TerminateJobFlows(new TerminateJobFlowsRequest());
                if(!(response.HttpStatusCode.Equals(200)))
                {
                    throw new Exception("Request failed. Check the body of your request.");
                }
                else
                {
                    deprovisionResult.EndUserMessage = "Success. Please wait 10-20 minutes for resources to be completely stopped.";
                }
               
            }
            catch (Exception e)
            {
                deprovisionResult.EndUserMessage = e.Message;
            }

            return deprovisionResult;
        }

        // Provision EMR JobFlows
        // The idea here is that we can use Amazon Elastic Map Reduce to create, persist, and reuse EMR Job Workflows.
        // Input: AddonDeprovisionRequest request
        // Output: ProvisionAddOnResult
        public override ProvisionAddOnResult Provision(AddonProvisionRequest request)
        {
            var provisionResult = new ProvisionAddOnResult("") { IsSuccess = false };
            AddonManifest manifest = request.Manifest;
            string developerOptions = request.DeveloperOptions;

            try
            {
                AmazonElasticMapReduceClient client;
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

                // execute new job flow here!
                var response = client.RunJobFlow(CreateJobFlowRequest(devOptions));

                if (!(response.HttpStatusCode.Equals(200)))
                {
                    throw new Exception("Request failed. Check the body of your request.");
                }
                else
                {
                    provisionResult.EndUserMessage = string.Format("Success. JobFlow Id: '{0}'", response.JobFlowId);
                }
                
            }
            catch (Exception e)
            {
                provisionResult.EndUserMessage = e.Message;
            }

            return provisionResult;
        }

        // Testing Instance
        // Input: AddonTestRequest request
        // Output: OperationResult
        public override OperationResult Test(AddonTestRequest request)
        {
            AddonManifest manifest = request.Manifest;
            string developerOptions = request.DeveloperOptions;
            var testResult = new OperationResult { IsSuccess = false };
            var testProgress = "";

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
                    AmazonElasticMapReduceClient client;
                    var establishClientResult = EstablishClient(manifest, devOptions, out client);
                    if (!establishClientResult.IsSuccess)
                    {
                        return establishClientResult;
                    }
                    testProgress += establishClientResult.EndUserMessage;

                    client.DescribeJobFlows();
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

        /* Begin private methods */



        // TODO: We might be able to extend this. 
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
            catch (ArgumentException e)
            {
                result.EndUserMessage = e.Message;
                return result;
            }

            result.IsSuccess = true;
            result.EndUserMessage = progress;
            return result;
        }

        private OperationResult EstablishClient(AddonManifest manifest, EMRDeveloperOptions devOptions, out AmazonElasticMapReduceClient client)
        {
            OperationResult result;

            bool requireCreds;
            var accessKey = manifest.ProvisioningUsername;
            var secretAccessKey = manifest.ProvisioningPassword;

            var prop =
                manifest.Properties.First(
                    p => p.Key.Equals("requireDevCredentials", StringComparison.InvariantCultureIgnoreCase));

            if (bool.TryParse(prop.Value, out requireCreds) && requireCreds)
            {
                if (!ValidateDevCreds(devOptions))
                {
                    client = null;
                    result = new OperationResult()
                    {
                        IsSuccess = false,
                        EndUserMessage =
                            "The add on requires that developer credentials are specified but none were provided."
                    };
                    return result;
                }

                accessKey = devOptions.AccessKey;
                secretAccessKey = devOptions.SecretAccessKey;
            }

            client = new AmazonElasticMapReduceClient(accessKey, secretAccessKey);
            result = new OperationResult { IsSuccess = true };
            return result;
        }

        private RunJobFlowRequest CreateJobFlowRequest(EMRDeveloperOptions devOptions)
        {
            var placement = new PlacementType()
            {
                AvailabilityZone = devOptions.AvailabilityZone
            };

            var config = new JobFlowInstancesConfig()
            {
                Ec2KeyName = devOptions.Ec2KeyName,
                InstanceCount = devOptions.InstanceCount,
                // in order to reuse the EMR JobFlow for future use, make sure this is set to TRUE.
                KeepJobFlowAliveWhenNoSteps = devOptions.KeepJobFlowaliveWhenNoSteps,
                MasterInstanceType = devOptions.MasterInstanceType,
                Placement = placement,
                SlaveInstanceType = devOptions.SlaveInstanceType
            };

            var hadoopjarstepconfig = new HadoopJarStepConfig()
            {
                Args = devOptions.Args,
                Jar = devOptions.Jar,
                MainClass = devOptions.MainClass
            };

            var stepconfig = new StepConfig()
            {
                Name = devOptions.stepName,
                ActionOnFailure = devOptions.ActionOnFailure,
                HadoopJarStep = hadoopjarstepconfig
            };

            var request = new RunJobFlowRequest()
            {
                // todo add in attributes here for devOptions stub creation
                Name = devOptions.JobFlowName,
                Instances = config,
                LogUri = devOptions.LogURI,
                Steps = {stepconfig}
            };

            return request;
        }
    }
}
