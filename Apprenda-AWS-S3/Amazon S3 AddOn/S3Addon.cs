using Amazon.S3;
using Amazon.S3.Model;
using Apprenda.Services.Logging;
using System;
using System.Linq;
using System.Net;

namespace Apprenda.SaaSGrid.Addons.AWS.S3
{
    public class S3Addon : AddonBase
    {
        private static readonly ILogger Log = LogManager.Instance().GetLogger(typeof(S3Addon));

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
                // heh, need to know which bucket to remove...
                developerOptions.BucketName = conInfo.BucketName;

                var establishClientResult = EstablishClient(manifest, developerOptions, out client);
                if (!establishClientResult.IsSuccess)
                {
                    deprovisionResult.EndUserMessage = establishClientResult.EndUserMessage;
                    return deprovisionResult;
                }

                var response =
                    client.DeleteBucket(new DeleteBucketRequest
                    {
                        BucketName = developerOptions.BucketName
                    });
                // 5/22/14 fixing amazon aws deprecation
                if (response.HttpStatusCode.Equals(HttpStatusCode.OK))
                {
                    var verificationResponse = client.ListBuckets(new ListBucketsRequest());
                    // 5/22/14 fixing amazaon aws deprecration
                    if (verificationResponse.Buckets.All(x => x.BucketName != conInfo.BucketName))
                    {
                        deprovisionResult.IsSuccess = true;
                        deprovisionResult.EndUserMessage = "Successfully deleted bucket: " + conInfo.BucketName;
                    }
                }
                else
                {
                    // error occurred during deletion
                    deprovisionResult.EndUserMessage = "Error during deprovision. Check S3 to ensure bucket was deleted.";
                }
            }
            catch (Exception e)
            {
                deprovisionResult.EndUserMessage = e.Message + e.StackTrace;
            }

            return deprovisionResult;
        }

        // Provision RDS Instance
        // Input: AddonDeprovisionRequest request
        // Output: ProvisionAddOnResult
        public override ProvisionAddOnResult Provision(AddonProvisionRequest request)
        {
            var provisionResult = new ProvisionAddOnResult("") { IsSuccess = false };
            var manifest = request.Manifest;

            // so here, we're going to have to
            //var developerOptions = request.DeveloperOptions;

            var developerParameters = request.DeveloperParameters;

            try
            {
                AmazonS3Client client;

                // to minimize the change, we need to take the DeveloperParameters property and convert it to the S3DeveloperOptions class
                var devOptions = S3DeveloperOptions.ParseWithParameters(developerParameters);

                // we're going to have to change this stuff here.
                //var parseOptionsResult = ParseDevOptions(developer, out devOptions);
                //if (!parseOptionsResult.IsSuccess)
                //{
                //    provisionResult.EndUserMessage = parseOptionsResult.EndUserMessage;
                //    return provisionResult;
                //}

                // this is a reference change
                //var establishClientResult = EstablishClient(manifest, S3DeveloperOptions.Parse(developerOptions), out client);

                // we'll change this underlying method
                var establishClientResult = EstablishClient(manifest, devOptions, out client);

                if (!establishClientResult.IsSuccess)
                {
                    provisionResult.EndUserMessage = establishClientResult.EndUserMessage;
                    return provisionResult;
                }

                var response = client.PutBucket(new PutBucketRequest
                {
                    BucketName = devOptions.BucketName,
                    BucketRegion = S3Region.US
                });

                if (response.HttpStatusCode != HttpStatusCode.OK)
                {
                    provisionResult.EndUserMessage = response.HttpStatusCode.ToString();
                    return provisionResult;
                }

                var verificationResponse = client.ListBuckets(new ListBucketsRequest());

                var bucket = verificationResponse.Buckets.Find(m => m.BucketName.Equals(devOptions.BucketName));

                if (bucket == null)
                {
                    provisionResult.EndUserMessage = "We aren't getting the bucket filtered here correctly.";
                    return provisionResult;
                }

                provisionResult.ConnectionData = "BucketName=" + devOptions.BucketName;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                Log.Error(e.Message + "\n" + e.StackTrace);
                provisionResult.IsSuccess = false;
                provisionResult.EndUserMessage = e.Message + "\n" + e.StackTrace;
                return provisionResult;
            }
            //
            provisionResult.IsSuccess = true;
            return provisionResult;
        }

        // Testing Instance
        // Input: AddonTestRequest request
        // Output: OperationResult
        public override OperationResult Test(AddonTestRequest request)
        {
            var provisionRequest = new AddonProvisionRequest { Manifest = request.Manifest, DeveloperOptions = request.DeveloperOptions };
            var manifest = request.Manifest;
            var developerOptions = request.DeveloperOptions;
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

                    testProgress += "Successfully connected. \n";
                    testResult.IsSuccess = true;
                    testResult.EndUserMessage = testProgress;

                    // ok and let let's try provisioning

                    var result = Provision(provisionRequest);

                    testResult.EndUserMessage += result.IsSuccess + "\n";
                    testResult.EndUserMessage += result.ConnectionData + "\n";
                    testResult.EndUserMessage += result.EndUserMessage + "\n";

                    var depResult = Deprovision(new AddonDeprovisionRequest { ConnectionData = result.ConnectionData, DeveloperOptions = provisionRequest.DeveloperOptions, Manifest = provisionRequest.Manifest });
                    if (depResult == null)
                    {
                        throw new ArgumentNullException("request");
                    }

                    testResult.EndUserMessage += depResult.IsSuccess + "\n";
                    testResult.EndUserMessage += depResult.EndUserMessage;
                }
                catch (Exception e)
                {
                    testResult.EndUserMessage = e.Message + e.StackTrace;
                }
            }
            else
            {
                testResult.EndUserMessage = "Missing required manifest properties (requireDevCredentials)";
            }

            return testResult;
        }

        private OperationResult EstablishClient(AddonManifest manifest, S3DeveloperOptions devOptions, out AmazonS3Client client)
        {
            OperationResult result;

            bool requireCreds;
            var manifestprops = manifest.GetProperties().ToDictionary(x => x.Key, x => x.Value);
            var accessKey = manifestprops["AWSClientKey"];
            var secretAccessKey = manifestprops["AWSSecretKey"];

            var prop =
                manifest.Properties.First(
                    p => p.Key.Equals("requireDevCredentials", StringComparison.InvariantCultureIgnoreCase));

            if (bool.TryParse(prop.Value, out requireCreds) && requireCreds)
            {
                if (!ValidateDevCreds(devOptions))
                {
                    client = null;
                    result = new OperationResult
                    {
                        IsSuccess = false,
                        EndUserMessage =
                            "The add on requires that developer credentials are specified but none were provided."
                    };
                    return result;
                }

                //accessKey = devOptions.AccessKey;
                //secretAccessKey = devOptions.SecretAccessKey;
                //regionEndpoint = devOptions.RegionEndpont;
            }
            var config = new AmazonS3Config { ServiceURL = @"http://s3.amazonaws.com" };
            client = new AmazonS3Client(accessKey, secretAccessKey, config);
            result = new OperationResult { IsSuccess = true };
            return result;
        }

        private static bool ValidateDevCreds(S3DeveloperOptions devOptions)
        {
            return !(string.IsNullOrWhiteSpace(devOptions.AccessKey) || string.IsNullOrWhiteSpace(devOptions.SecretAccessKey));
        }

        private static OperationResult ParseDevOptions(string developerOptions, out S3DeveloperOptions devOptions)
        {
            devOptions = null;
            var result = new OperationResult { IsSuccess = false };
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
    }
}