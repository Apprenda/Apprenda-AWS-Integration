using System;
using System.Collections.Generic;

namespace Apprenda.SaaSGrid.Addons.AWS.SQS
{
    public class DeveloperOptions
    {
        // Amazon Credentials. Required for IAM. 
        public string AccessKey { get; set; }
        public string SecretAccessKey { get; set; }
        public Dictionary<string, string> Attributes { get; set; }
        public string QueueName { get; set; }
       
        // Method takes in a string and parses it into a DeveloperOptions class.
        public static DeveloperOptions Parse(string developerOptions)
        {
            DeveloperOptions options = new DeveloperOptions();

            if (!string.IsNullOrWhiteSpace(developerOptions))
            {
                // splitting all entries into arrays of optionPairs
                var optionPairs = developerOptions.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var optionPair in optionPairs)
                {
                    // splitting all optionPairs into arrays of key/value denominations
                    var optionPairParts = optionPair.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    if (optionPairParts.Length == 2)
                    {
                        // check for attributes_
                        if (optionPairParts[0].Trim().ToLowerInvariant().Contains("attributes_"))
                        {
                            MapToOptionWithCollection(options, optionPairParts[0].Trim().ToLowerInvariant(), optionPairParts[1].Trim());
                        }
                        else
                        {
                            MapToOption(options, optionPairParts[0].Trim().ToLowerInvariant(), optionPairParts[1].Trim());
                        }
                    }
                    else
                    {
                        throw new ArgumentException(
                            string.Format(
                                "Unable to parse developer options which should be in the form of 'option=value&nextOption=nextValue'. The option '{0}' was not properly constructed",
                                optionPair));
                    }
                }
            }

            return options;
        }

        // Interior method takes in instance of DeveloperOptions (aptly named existingOptions) and maps them to the proper value. In essence, a setter method.
        private static void MapToOption(DeveloperOptions existingOptions, string key, string value)
        {
            if ("accesskey".Equals(key))
            {
                existingOptions.AccessKey = value;
                return;
            }

            if ("secretkey".Equals(key))
            {
                existingOptions.SecretAccessKey = value;
                return;
            }

           

            throw new ArgumentException(string.Format("The developer option '{0}' was not expected and is not understood.", key));
        }

        public static void MapToOptionWithCollection(DeveloperOptions existingOptions, string key, string value)
        {
            if (key.Contains("attributes_"))
                {
                    // trim the leading attributes and add the key
                    existingOptions.Attributes.Add(key.Replace("attributes_", ""), value);
                    return;
                }
                
                throw new ArgumentException(string.Format("The developer option '{0}' was not expected and is not understood.", key));
        }

        
    }
}