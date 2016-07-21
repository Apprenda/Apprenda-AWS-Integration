namespace Apprenda.SaaSGrid.Addons.AWS.SQS
{
    using Apprenda.SaaSGrid.Addons.AWS.Util;
    using System;
    using System.Linq;
    using Amazon.SQS;
    using Amazon.SQS.Model;
    using System.Threading;

    public class SqsAddOn : AddonBase
    {
        // Deprovision RDS Instance
        // Input: AddonDeprovisionRequest request
        // Output: OperationResult
        public override OperationResult Deprovision(AddonDeprovisionRequest _request)
        {
            var connectionData = _request.ConnectionData;
            // changing to overloaded constructor - 5/22/14
            var deprovisionResult = new ProvisionAddOnResult(connectionData);
            var manifest = _request.Manifest;
            //var devOptions = request.DeveloperOptions;
            try
            {
                var conInfo = SQSConnectionInfo.Parse(connectionData);
                var developerOptions = SQSDeveloperOptions.Parse(_request.DeveloperParameters);
                var client = EstablishClient(manifest);
                client.DeleteQueue(new DeleteQueueRequest()
                                       {
                                           QueueUrl = conInfo.QueueURL
                                       });
                do  {
                        var verificationResponse = client.GetQueueUrl(new GetQueueUrlRequest()
                        {
                            QueueName = conInfo.QueueName,
                            QueueOwnerAWSAccountId = developerOptions.AccessKey,
                        });
                        // 5/22/14 fixing amazaon aws deprecration
                        if (verificationResponse.QueueUrl == null)
                        {
                            deprovisionResult.IsSuccess = true;
                            break;
                        }
                        Thread.Sleep(TimeSpan.FromSeconds(10d));

                    } while (true);
                }
            
            catch (QueueDoesNotExistException)
            {
                deprovisionResult.IsSuccess = true;
            }
            catch (QueueDeletedRecentlyException)
            {
                deprovisionResult.IsSuccess = true;
            }
            catch (Exception e)
            {
                deprovisionResult.EndUserMessage = e.Message;
            }

            return deprovisionResult;
        }

        // Provision RDS Instance
        // Input: AddonDeprovisionRequest request
        // Output: ProvisionAddOnResult
        public override ProvisionAddOnResult Provision(AddonProvisionRequest _request)
        {
            // i think this is a bug. but I'm going to throw an empty string to it to clear the warning.
            var provisionResult = new ProvisionAddOnResult("");
            var manifest = _request.Manifest;
            var options = SQSDeveloperOptions.Parse(_request.DeveloperParameters);

            try
            {
                var client = EstablishClient(manifest);
                var response = client.CreateQueue(CreateQueueRequest(options));
                if (response.QueueUrl != null)
                {
                    do
                    {
                        var verificationResponse = client.GetQueueAttributes(new GetQueueAttributesRequest()
                            {
                                QueueUrl = response.QueueUrl
                            });
                        if(verificationResponse.Attributes != null)
                        {
                            var conInfo = new SQSConnectionInfo()
                            {
                                QueueName = options.QueueName,
                                QueueURL = response.QueueUrl
                            };
                            provisionResult.IsSuccess = true;
                            provisionResult.ConnectionData = conInfo.ToString();
                            break;
                        }
                        Thread.Sleep(TimeSpan.FromSeconds(10d));
                    } while (true);
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
        public override OperationResult Test(AddonTestRequest _request)
        {
            var manifest = _request.Manifest;
            var testResult = new OperationResult { IsSuccess = false };

            if (manifest.Properties != null && manifest.Properties.Any())
            {
                if (!ValidateManifest(manifest))
                {
                    testResult.EndUserMessage =
                        "Manifest validation failed. Check to make sure you have the proper credentials in your addon configuration.";
                    return testResult;
                }
                //var devOptions = SQSDeveloperOptions.Parse(request.DeveloperParameters);
                try
                {
                    var client = EstablishClient(manifest);
                    client.ListQueues(new ListQueuesRequest {
                                              // QueueNamePrefix is optional
                                          });
                    testResult.IsSuccess = true;
                    testResult.EndUserMessage = "Validation succeeded.";
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

        private static bool ValidateManifest(IAddOnDefinition _manifest)
        {
            return !string.IsNullOrWhiteSpace(_manifest.ProvisioningUsername) && !string.IsNullOrWhiteSpace(_manifest.ProvisioningPassword);
        }

        private static AmazonSQSClient EstablishClient(IAddOnDefinition _manifest)
        {
            var accessKey = _manifest.ProvisioningUsername;
            var secretAccessKey = _manifest.ProvisioningPassword;
            var regionEndpoint = AwsUtils.ParseRegionEndpoint(_manifest.ProvisioningLocation);
            var config = new AmazonSQSConfig() { RegionEndpoint = regionEndpoint };
            return new AmazonSQSClient(accessKey, secretAccessKey, config);
        }

        private static CreateQueueRequest CreateQueueRequest(SQSDeveloperOptions _devOptions)
        {
            var request = new CreateQueueRequest
            {
                // TODO - need to determine where defaults are used, and then not create the constructor where value is null (to use default)
               QueueName = _devOptions.QueueName,
               Attributes = _devOptions.Attributes.ToDict()
            };
            return request;
        }
    }
}
