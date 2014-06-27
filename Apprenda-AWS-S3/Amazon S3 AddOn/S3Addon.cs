using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon_Base_Addon;
using Apprenda.SaaSGrid.Addons;
using Amazon.S3.Model;
using Amazon.S3;

namespace Amazon_S3_AddOn
{
    public class S3Addon : Addon
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
                AmazonS3Client client;
                var conInfo = ConnectionInfo.Parse(connectionData);
                var developerOptions = S3DeveloperOptions.Parse(devOptions);

                var establishClientResult = EstablishClient(manifest, developerOptions, out client);
                if (!establishClientResult.IsSuccess)
                {
                    deprovisionResult.EndUserMessage = establishClientResult.EndUserMessage;
                    return deprovisionResult;
                }

                var response =
                    client.DeleteBucket(new DeleteBucketRequest()
                    {
                        // TODO- add in developer options
                        BucketName = developerOptions.BucketName,
                        BucketRegion = developerOptions.BucketRegion,
                        UseClientRegion = developerOptions.UseClientRegion
                    });
                // 5/22/14 fixing amazon aws deprecation
                if (!response.HttpStatusCode.Equals(200))
                {
                    do
                    {
                        var verificationResponse = client.ListBuckets(new ListBucketsRequest());
                        // 5/22/14 fixing amazaon aws deprecration
                        if (!verificationResponse.Buckets.Any())
                        {
                            deprovisionResult.IsSuccess = true;
                            break;
                        }
                       

                    } while (true);
                }
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
            string developerOptions = request.DeveloperOptions;

            try
            {
                AmazonS3Client client;
                S3DeveloperOptions devOptions;

                var parseOptionsResult = ParseDevOptions(developerOptions, out devOptions);
                if (!parseOptionsResult.IsSuccess)
                {
                    provisionResult.EndUserMessage = parseOptionsResult.EndUserMessage;
                    return provisionResult;
                }

                var establishClientResult = EstablishClient(manifest, S3DeveloperOptions.Parse(developerOptions), out client);
                if (!establishClientResult.IsSuccess)
                {
                    provisionResult.EndUserMessage = establishClientResult.EndUserMessage;
                    return provisionResult;
                }

                var response = client.PutBucket(CreatePutBucketRequest(devOptions));
                // fix 5/22/14 resolves amazon aws deprecation
                if (!response.HttpStatusCode.Equals(200))
                {
                    //var conInfo = new ConnectionInfo()
                    //{
                    //    DbInstanceIdentifier = devOptions.DbInstanceIndentifier
                    //};
                    //provisionResult.IsSuccess = true;
                    //provisionResult.ConnectionData = conInfo.ToString();
                    //Thread.Sleep(TimeSpan.FromMinutes(6));
                        var verificationResponse = client.ListBuckets(new ListBucketsRequest());
                        // fix on next few lines 5/22/14 resolve amazon aws deprecation.
                        if (verificationResponse.Buckets.Any())
                        {
                            provisionResult.IsSuccess = true;
                        }
                }
            }
            catch (Exception e)
            {
                provisionResult.EndUserMessage = e.Message;
            }

            return provisionResult;
        }

        private PutBucketRequest CreatePutBucketRequest(S3DeveloperOptions devOptions)
        {
            return new PutBucketRequest()
            {
                BucketName = devOptions.BucketName,
                BucketRegion = devOptions.BucketRegion,
                BucketRegionName = devOptions.BucketRegionName
            };
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
                S3DeveloperOptions devOptions;

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
                    AmazonS3Client client;
                    var establishClientResult = EstablishClient(manifest, devOptions, out client);
                    if (!establishClientResult.IsSuccess)
                    {
                        return establishClientResult;
                    }
                    testProgress += establishClientResult.EndUserMessage;

                    client.ListBuckets();
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


        private OperationResult EstablishClient(AddonManifest manifest, DeveloperOptions devOptions, out AmazonS3Client client)
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

            client = new AmazonS3Client(accessKey, secretAccessKey);
            result = new OperationResult { IsSuccess = true };
            return result;
        }

        // TODO: We might be able to extend this. 
        private bool ValidateDevCreds(DeveloperOptions devOptions)
        {
            return !(string.IsNullOrWhiteSpace(devOptions.AccessKey) || string.IsNullOrWhiteSpace(devOptions.SecretAccessKey));
        }

        private OperationResult ParseDevOptions(string developerOptions, out S3DeveloperOptions devOptions)
        {
            devOptions = null;
            var result = new OperationResult() { IsSuccess = false };
            var progress = "";

            try
            {
                progress += "Parsing developer options...\n";
                devOptions = S3DeveloperOptions.Parse(developerOptions);
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
    }
}
