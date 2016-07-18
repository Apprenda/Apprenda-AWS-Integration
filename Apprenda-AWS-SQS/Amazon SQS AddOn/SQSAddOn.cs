using System;
using System.Linq;
using Amazon.SQS;
using Amazon.SQS.Model;
using System.Threading;
using Amazon;


namespace Apprenda.SaaSGrid.Addons.AWS.SQS
{
    using Amazon.EC2.Model;

    public class SQSAddOn : AddonBase
    {
        // Deprovision RDS Instance
        // Input: AddonDeprovisionRequest request
        // Output: OperationResult
        public override OperationResult Deprovision(AddonDeprovisionRequest request)
        {
            var connectionData = request.ConnectionData;
            // changing to overloaded constructor - 5/22/14
            var deprovisionResult = new ProvisionAddOnResult(connectionData);
            var manifest = request.Manifest;
            //var devOptions = request.DeveloperOptions;
            try
            {
                AmazonSQSClient client;
                var conInfo = SQSConnectionInfo.Parse(connectionData);
                //var developerOptions = DeveloperOptions.Parse(devOptions);
                var developerOptions = SQSDeveloperOptions.Parse(request.DeveloperParameters);
                var establishClientResult = this.EstablishClient(manifest, developerOptions, out client);
                if (!establishClientResult.IsSuccess)
                {
                    deprovisionResult.EndUserMessage = establishClientResult.EndUserMessage;
                    return deprovisionResult;
                }

                var response =
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
        public override ProvisionAddOnResult Provision(AddonProvisionRequest request)
        {
            // i think this is a bug. but I'm going to throw an empty string to it to clear the warning.
            var provisionResult = new ProvisionAddOnResult("");
            AddonManifest manifest = request.Manifest;
            SQSDeveloperOptions options = SQSDeveloperOptions.Parse(request.DeveloperParameters);

            try
            {
                AmazonSQSClient client;
                var establishClientResult = this.EstablishClient(manifest, options, out client);
                if (!establishClientResult.IsSuccess)
                {
                    provisionResult.EndUserMessage = establishClientResult.EndUserMessage;
                    return provisionResult;
                }

                var response = client.CreateQueue(CreateQueueRequest(options));
                if (response.QueueUrl != null)
                {
                    do
                    {
                        var verificationResponse = client.GetQueueAttributes(new GetQueueAttributesRequest()
                            {
                                QueueUrl = response.QueueUrl
                            });
                        // fix on next few lines 5/22/14 resolve amazon aws deprecation.
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
        public override OperationResult Test(AddonTestRequest request)
        {
            var manifest = request.Manifest;
            var testResult = new OperationResult { IsSuccess = false };
            var testProgress = "";

            if (manifest.Properties != null && manifest.Properties.Any())
            {
                testProgress += "Evaluating required manifest properties...\n";
                if (!this.ValidateManifest(manifest, out testResult))
                {
                    return testResult;
                }
                var devOptions = SQSDeveloperOptions.Parse(request.DeveloperParameters);
                try
                {
                    testProgress += "Establishing connection to AWS...\n";
                    AmazonSQSClient client;
                    var establishClientResult = this.EstablishClient(manifest, devOptions, out client);
                    if (!establishClientResult.IsSuccess)
                    {
                        return establishClientResult;
                    }
                    testProgress += establishClientResult.EndUserMessage;
                    var queueResults = client.ListQueues(new ListQueuesRequest()
                    {
                        // QueueNamePrefix is optional
                    });
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
        private bool ValidateDevCreds(SQSDeveloperOptions devOptions)
        {
            return !(string.IsNullOrWhiteSpace(devOptions.AccessKey) || string.IsNullOrWhiteSpace(devOptions.SecretAccessKey));
        }

        private OperationResult EstablishClient(AddonManifest manifest, SQSDeveloperOptions devOptions, out AmazonSQSClient client)
        {
            OperationResult result;

            bool requireCreds;
            var manifestprops = manifest.GetProperties().ToDictionary(x=>x.Key, x=>x.Value);
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
            }
            AmazonSQSConfig config = new AmazonSQSConfig() { RegionEndpoint = RegionEndpoint.USEast1 };
            client = new AmazonSQSClient(AccessKey, SecretAccessKey, config);
            result = new OperationResult { IsSuccess = true };
            return result;
        }

        private CreateQueueRequest CreateQueueRequest(SQSDeveloperOptions devOptions)
        {
            var request = new CreateQueueRequest()
            {
                // TODO - need to determine where defaults are used, and then not create the constructor where value is null (to use default)
               QueueName = devOptions.QueueName,
               Attributes = devOptions.Attributes.ToDict()
            };
            return request;
        }
    }
}
