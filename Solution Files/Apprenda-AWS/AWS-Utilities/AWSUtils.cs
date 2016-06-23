/***
 * This class contains helper methods that can be used across projects.
 * Author: Chris Dutra
 * Last Updated: 3.13.2015
 */

using Amazon;
using System;

namespace Apprenda.SaaSGrid.Addons.AWS.Util
{
    public class AWSUtils
    {
        private static readonly RegionEndpoint DefaultRegion = RegionEndpoint.APNortheast1;

        // this is a commonly used method to determine the users Region Endpoint
        // for provisioning instances.
        public static RegionEndpoint ParseRegionEndpoint(string input, bool defaultOnError = false)
        {
            // If defaultOnError is set to true, the default will always be USEast1, to be safe.
            if (input.ToLowerInvariant().Equals("useast1")) return RegionEndpoint.USEast1;
            if (input.ToLowerInvariant().Equals("uswest1")) return RegionEndpoint.USWest1;
            if (input.ToLowerInvariant().Equals("uswest2")) return RegionEndpoint.USWest2;
            if (input.ToLowerInvariant().Equals("usgovcloudwest1")) return RegionEndpoint.USGovCloudWest1;
            if (input.ToLowerInvariant().Equals("saeast1")) return RegionEndpoint.SAEast1;
            if (input.ToLowerInvariant().Equals("eucentral1")) return RegionEndpoint.EUCentral1;
            if (input.ToLowerInvariant().Equals("euwest1")) return RegionEndpoint.EUWest1;
            if (input.ToLowerInvariant().Equals("apnortheast1")) return RegionEndpoint.APNortheast1;
            if (input.ToLowerInvariant().Equals("apsoutheast1")) return RegionEndpoint.APSoutheast1;
            if (input.ToLowerInvariant().Equals("apsoutheast2")) return RegionEndpoint.APSoutheast2;
            // just return the default if we can't parse it correctly
            if (defaultOnError) return DefaultRegion; throw new Exception("Unable to determine AWS Region");
        }
    }
}