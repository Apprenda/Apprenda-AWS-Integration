using System;
using System.Linq;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon;
using System.Threading;

namespace Apprenda.SaaSGrid.Addons.AWS.SNS
{
    public class SNSAddOn : AddonBase
    {
        public override OperationResult Deprovision(AddonDeprovisionRequest request)
        {
            string connectionData = request.ConnectionData;
            // changing to overloaded constructor - 5/22/14
            var deprovisionResult = new ProvisionAddOnResult(connectionData);
            AddonManifest manifest = request.Manifest;
            string devOptions = request.DeveloperOptions;
            try
            {
                AmazonSimpleNotificationServiceClient client;
                var conInfo = ConnectionInfo.Parse(connectionData);
                var developerOptions = DeveloperOptions.Parse(devOptions);
                var establishClientResult = EstablishClient(manifest, developerOptions, out client);
                if (!establishClientResult.IsSuccess)
                {
                    deprovisionResult.EndUserMessage = establishClientResult.EndUserMessage;
                    return deprovisionResult;
                }
                var response =
                    client.DeleteTopic(new DeleteTopicRequest()
                    {
                        TopicArn = conInfo.TopicArn
                    });
                if (response.HttpStatusCode != null)
                {
                    do
                    {
                        // ok, to verify deletion, we need to list all of the topics and search for the one we just deleted.
                        // if it's still there, its probably in queue to be deleted, we'll sleep the thread and give it a minute.
                        // once its gone, we'll return true.
                        // if after an intolerable amount of time the queue is still there, throw an error.
                        var verificationResponse = client.ListTopics(new ListTopicsRequest());
                        // if there are no topics, ok!
                        if (verificationResponse.Topics.Count == 0)
                        {
                            deprovisionResult.IsSuccess = true;
                            break;
                        }
                        // if there are existing topics, search for the one we just deleted.
                        if(verificationResponse.Topics.Find(m => m.TopicArn.Equals(conInfo.TopicArn)) == null)
                        {
                            deprovisionResult.IsSuccess = true;
                            break;
                        }
                        // otherwise, the topic still exists and we need to wait a little longer.
                        Thread.Sleep(TimeSpan.FromSeconds(10d));

                    } while (true);
                }
            }
            catch (Exception e)
            {
                deprovisionResult.EndUserMessage += "An error occurred during deletion. Your SNS queue may be deleted, but we were unable to verify. Please check your AWS Console."; 
                deprovisionResult.EndUserMessage += e.Message;
            }
            return deprovisionResult;
        }

        // Provision SNS Topic
        // Input: AddonDeprovisionRequest request
        // Output: ProvisionAddOnResult
        public override ProvisionAddOnResult Provision(AddonProvisionRequest request)
        {
            // i think this is a bug. but I'm going to throw an empty string to it to clear the warning.
            var provisionResult = new ProvisionAddOnResult("");
            AddonManifest manifest = request.Manifest;
            string developerOptions = request.DeveloperOptions;
            try
            {
                AmazonSimpleNotificationServiceClient client;
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
                var response = client.CreateTopic(CreateTopicRequest(devOptions));
                do
                    {
                        var verificationResponse = client.GetTopicAttributes(new GetTopicAttributesRequest()
                            {
                                TopicArn = response.TopicArn
                            });
                        // ok so the attributes works as follows:
                        // attributes[0] - topicarn
                        // attributes[1] - owner
                        // attributes[2] - policy
                        // attributes[3] - displayname
                        // attributes[4] - subscriptionspending
                        // attributes[5] - subscriptionsconfirmed
                        // attributes[6] - subscriptionsdeleted
                        // attributes[7] - deliverypolicy
                        // attributes[8] - effectivedeliverypolicy
                        if (verificationResponse.Attributes["TopicArn"].Equals(response.TopicArn))
                        {
                            var conInfo = new ConnectionInfo()
                            {
                                TopicArn = verificationResponse.Attributes["TopicArn"],
                                QueueName = verificationResponse.Attributes["DisplayName"]
                                
                            };
                            provisionResult.IsSuccess = true;
                            provisionResult.ConnectionData = conInfo.ToString();
                            break;
                        }
                        Thread.Sleep(TimeSpan.FromSeconds(10d));
                    } while (true);
                
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
                    AmazonSimpleNotificationServiceClient client;
                    var establishClientResult = EstablishClient(manifest, devOptions, out client);
                    if (!establishClientResult.IsSuccess)
                    {
                        return establishClientResult;
                    }
                    testProgress += establishClientResult.EndUserMessage;

                    var platformApps = client.ListPlatformApplications();
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

        // TODO: We might be able to extend this. 
        private bool ValidateDevCreds(DeveloperOptions devOptions)
        {
            return !(string.IsNullOrWhiteSpace(devOptions.AccessKey) || string.IsNullOrWhiteSpace(devOptions.SecretAccessKey));
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
            catch (ArgumentException e)
            {
                result.EndUserMessage = e.Message;
                return result;
            }

            result.IsSuccess = true;
            result.EndUserMessage = progress;
            return result;
        }

        private OperationResult EstablishClient(AddonManifest manifest, DeveloperOptions devOptions, out AmazonSimpleNotificationServiceClient client)
        {
            OperationResult result;

            bool requireCreds;
            var manifestprops = manifest.GetProperties().ToDictionary(x => x.Key, x => x.Value);
            var AccessKey = manifestprops["AWSClientKey"];
            var SecretAccessKey = manifestprops["AWSSecretKey"];
            var _RegionEndpoint = manifestprops["AWSRegionEndpoint"];

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

                //accessKey = devOptions.AccessKey;
                //secretAccessKey = devOptions.SecretAccessKey;
            }
            AmazonSimpleNotificationServiceConfig config = new AmazonSimpleNotificationServiceConfig() { RegionEndpoint = RegionEndpoint.USEast1 };
            client = new AmazonSimpleNotificationServiceClient(AccessKey, SecretAccessKey, config);
            result = new OperationResult { IsSuccess = true };
            return result;
        }

        private CreateTopicRequest CreateTopicRequest(DeveloperOptions devOptions)
        {
            var request = new CreateTopicRequest()
            {
                // TODO - need to determine where defaults are used, and then not create the constructor where value is null (to use default)
                Name = devOptions.TopicName
                // These are required values.
            };  
            return request;
        }

        private CreatePlatformApplicationRequest CreatePlatformApplicaitonRequest(DeveloperOptions devOptions)
        {
            var request = new CreatePlatformApplicationRequest()
            {
                // TODO - need to determine where defaults are used, and then not create the constructor where value is null (to use default)
                Name = devOptions.PlatformApplicationName,
                Attributes = devOptions.PlatformAttributes,
                Platform = devOptions.MessagingPlatform
                // These are required values.
            };
            return request;
        }


        private CreatePlatformEndpointRequest CreatePlatformEndpointRequest(DeveloperOptions devOptions)
        {
            var request = new CreatePlatformEndpointRequest()
            {
                PlatformApplicationArn = devOptions.PlatformApplicationArn,
                CustomUserData = devOptions.CustomUserData,
                Token = devOptions.Token,
                Attributes = devOptions.EndpointAttributes
            };

            return request;
        }
    }
}
