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
    using System.Net;
    using Amazon;
    using Amazon.S3;
    using Amazon.S3.Model;
    using Util;
    using Services.Logging;

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
        /// <param name="_request">
        /// The request.
        /// </param>
        /// <returns>
        /// The <see cref="OperationResult"/>.
        /// </returns>
        public override OperationResult Deprovision(AddonDeprovisionRequest _request)
        {
            var connectionData = _request.ConnectionData;
            var deprovisionResult = new OperationResult();
            var manifest = _request.Manifest;
            var devParameters = _request.DeveloperParameters;
            try
            {
                var conInfo = S3ConnectionInfo.Parse(connectionData);
                var devOptions = S3DeveloperOptions.Parse(devParameters, manifest);
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
        /// <param name="_request">
        /// The request.
        /// </param>
        /// <returns>
        /// The <see cref="ProvisionAddOnResult"/>.
        /// </returns>
        public override ProvisionAddOnResult Provision(AddonProvisionRequest _request)
        {
            var provisionResult = new ProvisionAddOnResult(string.Empty) { IsSuccess = false };
            try
            {
                var devOptions = S3DeveloperOptions.Parse(_request.DeveloperParameters, _request.Manifest);
                var client = EstablishClient(_request.Manifest);
                var putRequest = new PutBucketRequest
                                      {
                                          BucketName = devOptions.BucketName,
                                          BucketRegion =
                                              TranslateRegionEndpoints(
                                                  _request.Manifest.ProvisioningLocation,
                                                  devOptions)
                                      };
                var response = client.PutBucket(putRequest);

                if (response.HttpStatusCode != HttpStatusCode.OK)
                {
                    provisionResult.EndUserMessage = response.HttpStatusCode.ToString();
                    return provisionResult;
                }

                var verificationResponse = client.ListBuckets(new ListBucketsRequest());

                var bucket = verificationResponse.Buckets.Find(_m => _m.BucketName.Equals(devOptions.BucketName));

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

        private static S3Region TranslateRegionEndpoints(string _manifestLocation, S3DeveloperOptions _options)
        {
            return Translate(_options.UseClientRegion ? AwsUtils.ParseRegionEndpoint(_manifestLocation, true) : AwsUtils.ParseRegionEndpoint(_options.BucketRegionName, true));
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
                    var depResult = this.Deprovision(new AddonDeprovisionRequest
                    { ConnectionData = result.ConnectionData, DeveloperParameters = provisionRequest.DeveloperParameters, Manifest = provisionRequest.Manifest });
                    if (depResult == null)
                    {
                        throw new ArgumentNullException(nameof(_request));
                    }
                    testResult.IsSuccess = (result.IsSuccess && depResult.IsSuccess);
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
        /// <param name="_manifest">
        /// The manifest.
        /// </param>
        /// <returns>
        /// The <see cref="OperationResult"/>.
        /// </returns>
        private static AmazonS3Client EstablishClient(IAddOnDefinition _manifest)
        {
            var accessKey = _manifest.ProvisioningUsername;
            var secretAccessKey = _manifest.ProvisioningPassword;
            var regionEndpoint = AwsUtils.ParseRegionEndpoint(_manifest.ProvisioningLocation, true);
            var config = new AmazonS3Config { ServiceURL = @"http://s3.amazonaws.com", RegionEndpoint = regionEndpoint};
            return new AmazonS3Client(accessKey, secretAccessKey, config);
        }

        /// <summary>
        /// The test developer parameters.
        /// </summary>
        /// <param name="_devParams">
        /// The dev params.
        /// </param>
        /// <param name="_devOptions">
        /// The dev options.
        /// </param>
        /// <returns>
        /// The <see cref="OperationResult"/>.
        /// </returns>
        private static OperationResult TestDeveloperParameters(IEnumerable<AddonParameter> _devParams, out S3DeveloperOptions _devOptions)
        {
            _devOptions = new S3DeveloperOptions();
            var result = new OperationResult();
            foreach (var param in _devParams)
            {
                if (!param.Key.ToLowerInvariant().Equals("bucketname") || (param.Value.Length <= 0)) continue;
                result.IsSuccess = true;
                _devOptions.BucketName = param.Value;
            }
            return result;
        }
    }
}