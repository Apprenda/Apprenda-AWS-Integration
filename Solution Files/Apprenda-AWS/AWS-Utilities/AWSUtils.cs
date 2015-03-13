/***
 * This class contains helper methods that can be used across projects.
 * Author: Chris Dutra
 * Last Updated: 3.13.2015
 */

using Amazon;

namespace Apprenda.SaaSGrid.Addons.AWS.Util
{
    public class AWSUtils
    {
        // this is a commonly used method to determine the users Region Endpoint
        // for provisioning instances.
        public static RegionEndpoint ParseRegionEndpoint(string input)
        {
            // the default will always be USEast1, to be safe.
            return RegionEndpoint.USEast1;
        }
    }
}