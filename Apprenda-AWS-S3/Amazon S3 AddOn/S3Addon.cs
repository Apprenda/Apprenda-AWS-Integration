// --------------------------------------------------------------------------------------------------------------------
// <copyright file="S3Addon.cs" company="Apprenda">
//   MIT License
// </copyright>
// <summary>
//   The s 3 addon.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Apprenda.SaaSGrid.Addons.AWS.S3
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using Amazon.S3;
    using Amazon.S3.Model;
    using Apprenda.SaaSGrid.Addons.AWS.Util;
    using Apprenda.Services.Logging;

    /// <summary>
    /// The s 3 add-on.
    /// </summary>
    public class S3Addon : AddonBase
    {
        /// <summary>
        /// The log.
        /// </summary>
        private static readonly ILogger Log = LogManager.Instance().GetLogger(typeof(S3Addon));

        /// <summary>
        /// Removes an instance of the add-on, deleting the S3 bucket.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <returns>
        /// The <see cref="OperationResult"/>.
        /// </returns>
        public override OperationResult Deprovision(AddonDeprovisionRequest request)
        {
            var connectionData = request.ConnectionData;
            var deprovisionResult = new ProvisionAddOnResult(connectionData);
            var manifest = request.Manifest;
            var devParameters = request.DeveloperParameters;
            try
            {
                AmazonS3Client client;
                var conInfo = S3ConnectionInfo.Parse(connectionData);
                var devOptions = S3DeveloperOptions.ParseWithParameters(devParameters);
                devOptions.BucketName = conInfo.BucketName;
                var establishClientResult = this.EstablishClient(manifest, devOptions, out client);
                if (!establishClientResult.IsSuccess)
                {
                    deprovisionResult.EndUserMessage = establishClientResult.EndUserMessage;
                    return deprovisionResult;
                }

                var response =
                    client.DeleteBucket(new DeleteBucketRequest
                    {
                        BucketName = devOptions.BucketName
                    });
                if (response.HttpStatusCode.Equals(HttpStatusCode.OK))
                {
                    while (true)
                    {
                        var verificationResponse = client.ListBuckets(new ListBucketsRequest());
                        if (verificationResponse.Buckets.All(x => x.BucketName != conInfo.BucketName))
                        {
                            deprovisionResult.IsSuccess = true;
                            deprovisionResult.EndUserMessage = "Successfully deleted bucket: " + conInfo.BucketName;
                            return deprovisionResult;
                        } 
                    }
                }
                else
                {
                    deprovisionResult.EndUserMessage = "We sent the deleted the bucket, but didn't get confirmation back yet. Check S3 to ensure bucket was deleted.";
                }
            }
            catch (Exception e)
            {
                deprovisionResult.EndUserMessage =
                    "There was an error while deprovisioning your addon. Please check with your platform operator. \n";
                deprovisionResult.EndUserMessage += e.Message;
            }

            return deprovisionResult;
        }

        /// <summary>
        /// The provision.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <returns>
        /// The <see cref="ProvisionAddOnResult"/>.
        /// </returns>
        public override ProvisionAddOnResult Provision(AddonProvisionRequest request)
        {
            var provisionResult = new ProvisionAddOnResult(string.Empty) { IsSuccess = false };
            var manifest = request.Manifest;
            var developerParameters = request.DeveloperParameters;

            try
            {
                AmazonS3Client client;
                var devOptions = S3DeveloperOptions.ParseWithParameters(developerParameters);
                var establishClientResult = this.EstablishClient(manifest, devOptions, out client);

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

                var connInfo = new S3ConnectionInfo
                {
                    BucketName = devOptions.BucketName
                };
                provisionResult.ConnectionData = connInfo.ToString();
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

        /// <summary>
        /// The test.
        /// </summary>
        /// <param name="request">
        /// The request.
        /// </param>
        /// <returns>
        /// The <see cref="OperationResult"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public override OperationResult Test(AddonTestRequest request)
        {
            var provisionRequest = new AddonProvisionRequest { Manifest = request.Manifest, DeveloperParameters = request.DeveloperParameters };
            var manifest = request.Manifest;
            var developerParameters = request.DeveloperParameters;
            var testResult = new OperationResult { IsSuccess = false };
            var testProgress = string.Empty;

            if (manifest.Properties != null && manifest.Properties.Any())
            {
                S3DeveloperOptions devOptions;

                testProgress += "Evaluating required manifest properties...\n";
                if (!ValidateManifest(manifest, out testResult))
                {
                    return testResult;
                }

                var parseOptionsResult = TestDeveloperParameters(developerParameters, out devOptions);
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

                    var depResult = Deprovision(new AddonDeprovisionRequest { ConnectionData = result.ConnectionData, DeveloperParameters = provisionRequest.DeveloperParameters, Manifest = provisionRequest.Manifest });
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

        /// <summary>
        /// The establish client.
        /// </summary>
        /// <param name="manifest">
        /// The manifest.
        /// </param>
        /// <param name="devOptions">
        /// The dev options.
        /// </param>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <returns>
        /// The <see cref="OperationResult"/>.
        /// </returns>
        private OperationResult EstablishClient(AddonManifest manifest, S3DeveloperOptions devOptions, out AmazonS3Client client)
        {
            OperationResult result;

            bool requireCreds;
            var manifestprops = manifest.GetProperties().ToDictionary(x => x.Key, x => x.Value);
            var accessKey = manifest.ProvisioningUsername;
            var secretAccessKey = manifest.ProvisioningPassword;
            var regionEndpoint = AWSUtils.ParseRegionEndpoint(manifest.ProvisioningLocation);

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
            }
            var config = new AmazonS3Config { ServiceURL = @"http://s3.amazonaws.com" };
            client = new AmazonS3Client(accessKey, secretAccessKey, config);
            result = new OperationResult { IsSuccess = true };
            return result;
        }

        /// <summary>
        /// The validate dev creds.
        /// </summary>
        /// <param name="devOptions">
        /// The dev options.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private static bool ValidateDevCreds(S3DeveloperOptions devOptions)
        {
            return !(string.IsNullOrWhiteSpace(devOptions.AccessKey) || string.IsNullOrWhiteSpace(devOptions.SecretAccessKey));
        }

        /// <summary>
        /// The test developer parameters.
        /// </summary>
        /// <param name="devParams">
        /// The dev params.
        /// </param>
        /// <param name="devOptions">
        /// The dev options.
        /// </param>
        /// <returns>
        /// The <see cref="OperationResult"/>.
        /// </returns>
        private static OperationResult TestDeveloperParameters(IEnumerable<AddonParameter> devParams, out S3DeveloperOptions devOptions)
        {
            devOptions = new S3DeveloperOptions();
            var result = new OperationResult();
            foreach (var param in devParams)
            {
                if (param.Key.ToLowerInvariant().Equals("bucketname"))
                {
                    if (param.Value.Length > 0)
                    {
                        // this is all of the required params we need, return true;
                        result.IsSuccess = true;
                        devOptions.BucketName = param.Value;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// The validate manifest.
        /// </summary>
        /// <param name="manifest">
        /// The manifest.
        /// </param>
        /// <param name="testResult">
        /// The test result.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
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

            if (!string.IsNullOrWhiteSpace(manifest.ProvisioningUsername) &&
                !string.IsNullOrWhiteSpace(manifest.ProvisioningPassword)) return true;
            testResult.IsSuccess = false;
            testResult.EndUserMessage = "Missing credentials 'provisioningUsername' & 'provisioningPassword' . These values needs to be provided as part of the manifest";
            return false;
        }
    }
}