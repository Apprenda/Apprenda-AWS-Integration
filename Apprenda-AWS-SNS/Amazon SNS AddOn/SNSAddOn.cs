using System;
using System.Linq;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon;
using System.Threading;

namespace Apprenda.SaaSGrid.Addons.AWS.SNS
{
    public class SnsAddOn : AddonBase
    {
        public override OperationResult Deprovision(AddonDeprovisionRequest _request)
        {
            var connectionData = _request.ConnectionData;
            // changing to overloaded constructor - 5/22/14
            var deprovisionResult = new ProvisionAddOnResult(connectionData);
            try
            {
                var conInfo = SnsConnectionInfo.Parse(connectionData);
                //var developerOptions = SNSDeveloperOptions.Parse(request.DeveloperParameters, request.Manifest);
                var client = EstablishClient(_request.Manifest);
                client.DeleteTopic(new DeleteTopicRequest{ TopicArn = conInfo.TopicArn });
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
                    if(verificationResponse.Topics.Find(_m => _m.TopicArn.Equals(conInfo.TopicArn)) == null)
                    {
                        deprovisionResult.IsSuccess = true;
                        break;
                    }
                    // otherwise, the topic still exists and we need to wait a little longer.
                    Thread.Sleep(TimeSpan.FromSeconds(10d));

                } while (true);
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
        public override ProvisionAddOnResult Provision(AddonProvisionRequest _request)
        {
            // i think this is a bug. but I'm going to throw an empty string to it to clear the warning.
            var provisionResult = new ProvisionAddOnResult("");
            try
            {
                var devOptions = SnsDeveloperOptions.Parse(_request.DeveloperParameters, _request.Manifest);
                var client = EstablishClient(_request.Manifest);
                var response = client.CreateTopic(CreateTopicRequest(devOptions));
                do
                    {
                        var verificationResponse = client.GetTopicAttributes(new GetTopicAttributesRequest()
                            {
                                TopicArn = response.TopicArn
                            });
                        if (verificationResponse.Attributes["TopicArn"].Equals(response.TopicArn))
                        {
                            var conInfo = new SnsConnectionInfo()
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
        public override OperationResult Test(AddonTestRequest _request)
        {
            var testResult = new OperationResult { IsSuccess = false };
            
            //var devOptions = SNSDeveloperOptions.Parse(_request.DeveloperParameters, _request.Manifest);
            try
            {
                var client = EstablishClient(_request.Manifest);
                //var platformApps = 
                client.ListPlatformApplications();
                // todo add more tests here.
                testResult.IsSuccess = true;
                testResult.EndUserMessage = "Test complete.";
            }
            catch (Exception e)
            {
                testResult.EndUserMessage = e.Message;
            }
            return testResult;
        }

        private static AmazonSimpleNotificationServiceClient EstablishClient(AddonManifest _manifest)
        {
            var accessKey = _manifest.ProvisioningUsername;
            var secretAccessKey = _manifest.ProvisioningPassword;
            // todo add location in RegionEndpoint
            var config = new AmazonSimpleNotificationServiceConfig { RegionEndpoint = RegionEndpoint.USEast1 };
            return new AmazonSimpleNotificationServiceClient(accessKey, secretAccessKey, config);
        }


        private static CreateTopicRequest CreateTopicRequest(SnsDeveloperOptions _devOptions)
        {
            var request = new CreateTopicRequest
            {
                Name = _devOptions.TopicName
            };  
            return request;
        }
    }
}
