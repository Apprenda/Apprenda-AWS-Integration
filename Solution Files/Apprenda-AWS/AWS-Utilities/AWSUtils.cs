/***
 * This class contains helper methods that can be used across projects.
 * Author: Chris Dutra
 * Last Updated: 7.20.2016
 */

using Amazon;
using System;

namespace Apprenda.SaaSGrid.Addons.AWS.Util
{
    public static class AwsUtils
    {
        private static readonly RegionEndpoint DefaultRegion = RegionEndpoint.USEast1;

        // this is a utility method to determine the users Region Endpoint. since we do not have the ability to force selections of parameters or properties in extensibility, we have
        // to be careful how we parse. one user may use the legacy "us-east-1" (which is still used by the AWS console today), and others may use the actual object classifer (ie. RegionEndpoint.USEast1)
        // this is a band-aid until we can introduce this improvement
        public static RegionEndpoint ParseRegionEndpoint(string _input, bool _defaultOnError = false)
        {
            // If defaultOnError is set to true, the default will always be USEast1, to be safe.
            if (_input.ToLowerInvariant().Equals("us-east-1") || _input.ToLowerInvariant().Contains("useast")) return RegionEndpoint.USEast1;
            if (_input.ToLowerInvariant().Equals("us-west-1") || _input.ToLowerInvariant().Contains("uswest"))  return RegionEndpoint.USWest1;
            if (_input.ToLowerInvariant().Equals("us-west-2")) return RegionEndpoint.USWest2;
            if (_input.ToLowerInvariant().Equals("us-govcloudwest-1") || _input.ToLowerInvariant().Contains("usgov")) return RegionEndpoint.USGovCloudWest1;
            if (_input.ToLowerInvariant().Equals("sa-east-1") || _input.ToLowerInvariant().Contains("saeast")) return RegionEndpoint.SAEast1;
            if (_input.ToLowerInvariant().Equals("eu-central-1") || _input.ToLowerInvariant().Contains("eucentral")) return RegionEndpoint.EUCentral1;
            if (_input.ToLowerInvariant().Equals("eu-west-1") || _input.ToLowerInvariant().Contains("euwest")) return RegionEndpoint.EUWest1;
            if (_input.ToLowerInvariant().Equals("ap-northeast-1") || _input.ToLowerInvariant().Contains("apnortheast")) return RegionEndpoint.APNortheast1;
            if (_input.ToLowerInvariant().Equals("ap-southeast-1") || _input.ToLowerInvariant().Contains("apsoutheast")) return RegionEndpoint.APSoutheast1;
            if (_input.ToLowerInvariant().Equals("ap-southeast-2")) return RegionEndpoint.APSoutheast2;
            // if it fails and _defaultOnError is true, just return the default region.
            if (_defaultOnError) return DefaultRegion; 
            throw new ArgumentException("Unable to determine AWS Region, could not parse {0}.", _input);
        }
    }
}