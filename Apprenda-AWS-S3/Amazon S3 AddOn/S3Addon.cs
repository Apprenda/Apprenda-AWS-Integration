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

    using Amazon;
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
            var deprovisionResult = new OperationResult();
            var manifest = request.Manifest;
            var devParameters = request.DeveloperParameters;
            try
            {
                var conInfo = S3ConnectionInfo.Parse(connectionData);
                var devOptions = S3DeveloperOptions.Parse(devParameters);
                devOptions.BucketName = conInfo.BucketName;
                var client = EstablishClient(manifest);
                client.DeleteBucket(new DeleteBucketRequest
                {
                    BucketName = devOptions.BucketName
                });
                deprovisionResult.IsSuccess = true;
                deprovisionResult.EndUserMessage = "Sent request to delete S3 bucket: " + conInfo.BucketName;
                return deprovisionResult;
            }
            catch (Exception e)
            {
                deprovisionResult.EndUserMessage = e.Message;
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
                var devOptions = S3DeveloperOptions.Parse(developerParameters);
                var client = EstablishClient(manifest);
                var response = client.PutBucket(new PutBucketRequest
                {
                    BucketName = devOptions.BucketName,
                    BucketRegion = TranslateRegionEndpoints(manifest, devOptions)
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


        #region S3 Region Translation

        // so we need to do this because S3 doesn't use the traditional AWS Region Endpoints. Hopefully this changes in the future.

        private static S3Region TranslateRegionEndpoints(AddonManifest _manifest, S3DeveloperOptions _options)
        {
            
            return Translate(_options.UseClientRegion ? AwsUtils.ParseRegionEndpoint(_manifest.ProvisioningLocation, true) : AwsUtils.ParseRegionEndpoint(_options.BucketRegion, true));
        }

        private static S3Region Translate(RegionEndpoint _s)
        {
            if (_s == RegionEndpoint.USEast1)
                return S3Region.US;
            if( _s == RegionEndpoint.APNortheast1)
                return S3Region.APN1;
            if (_s == RegionEndpoint.APNortheast2)
                return S3Region.APN2;
            if (_s == RegionEndpoint.APSoutheast1)
                return S3Region.APS1;
            if (_s == RegionEndpoint.APSoutheast2)
                return S3Region.APS2;
            if (_s == RegionEndpoint.CNNorth1)
                return S3Region.CN1;
            if (_s == RegionEndpoint.EUCentral1)
                return S3Region.EUC1;
            if (_s == RegionEndpoint.EUWest1)
                return S3Region.EU;
            if (_s == RegionEndpoint.SAEast1)
                return S3Region.SAE1;
            if (_s == RegionEndpoint.USGovCloudWest1)
                return S3Region.GOVW1;
            if (_s == RegionEndpoint.USWest1)
                return S3Region.USW1;
            if (_s == RegionEndpoint.USWest2)
                return S3Region.USW2;
            throw new ArgumentException("Unrecognized Region Endpoint.");
        }

        #endregion


        /// <summary>
        /// The test.
        /// </summary>
        /// <param name="_request">
        /// The request.
        /// </param>
        /// <returns>
        /// The <see cref="OperationResult"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// </exception>
        public override OperationResult Test(AddonTestRequest _request)
        {
            var provisionRequest = new AddonProvisionRequest { Manifest = _request.Manifest, DeveloperParameters = _request.DeveloperParameters };
            var manifest = _request.Manifest;
            var developerParameters = _request.DeveloperParameters;
            var testResult = new OperationResult { IsSuccess = false };
            {
                S3DeveloperOptions devOptions;
                var parseOptionsResult = TestDeveloperParameters(developerParameters, out devOptions);
                if (!parseOptionsResult.IsSuccess)
                {
                    return parseOptionsResult;
                }
                try
                {
                    EstablishClient(manifest);
                    var result = this.Provision(provisionRequest);
                    var depResult = this.Deprovision(new AddonDeprovisionRequest { ConnectionData = result.ConnectionData, DeveloperParameters = provisionRequest.DeveloperParameters, Manifest = provisionRequest.Manifest });
                    if (depResult == null)
                    {
                        throw new ArgumentNullException("_request");
                    }
                }
                catch (Exception e)
                {
                    testResult.EndUserMessage = "Error occured during testing. Exception: " + e.Message;
                    return testResult;
                }
            }
            
            return testResult;
        }

        /// <summary>
        /// The establish client.
        /// </summary>
        /// <param name="manifest">
        /// The manifest.
        /// </param>
        /// <returns>
        /// The <see cref="OperationResult"/>.
        /// </returns>
        private static AmazonS3Client EstablishClient(IAddOnDefinition manifest)
        {
            var accessKey = manifest.ProvisioningUsername;
            var secretAccessKey = manifest.ProvisioningPassword;
            var regionEndpoint = AwsUtils.ParseRegionEndpoint(manifest.ProvisioningLocation);
            var config = new AmazonS3Config { ServiceURL = @"http://s3.amazonaws.com", RegionEndpoint = regionEndpoint};
            return new AmazonS3Client(accessKey, secretAccessKey, config);
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
    }
}