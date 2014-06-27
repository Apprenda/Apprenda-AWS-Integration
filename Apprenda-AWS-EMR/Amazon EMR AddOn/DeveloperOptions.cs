using System;
using System.Collections.Generic;


/* This class is currently WIP, as I will work towards abstracting out reusable components for all amazon components 
 * 
 * Author: Chris Dutra
 */

namespace Amazon_Base_Addon
{
    public abstract class DeveloperOptions
    {
        // will be used in all amazon calls
        // Amazon Credentials. Required for IAM. 
        public string AccessKey { get; set; }
        public string SecretAccessKey { get; set; }

        // Method takes in a string and parses it into a DeveloperOptions class.

        //  public abstract DeveloperOptions Parse(string developerOptions);

        // -----------------------------------------------------------------------

        // Use this method to map all collections to their proper places.
        // Usage here is to confirm that the subsequent key is the same as the preceding key. 
        // This forces that the REST call ensure all collection parameters are grouped together.
        // Ex. This is good: (key1=value&key1=value2)
        // Ex. This is bad: (key1=value&key2=value2)
        // Ex. This is bad: (key1=value&key2=value2&key1=value3)

        //public abstract void MapToOptionWithCollection(DeveloperOptions existingOptions, string key, string value, string lastKey);

        // ------------------------------------------------------------------------
        
        // Interior method takes in instance of DeveloperOptions (aptly named existingOptions) and maps them to the proper value. In essence, a setter method.
        
        //public abstract void MapToOption(DeveloperOptions existingOptions, string key, string value);
    }
}
