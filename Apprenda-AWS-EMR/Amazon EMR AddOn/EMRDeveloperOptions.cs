using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon_Base_Addon;

namespace Amazon_EMR_AddOn
{
    class EMRDeveloperOptions : DeveloperOptions
    {
        public string Ec2KeyName { get; set; }
        public int InstanceCount { get; set; }
        public bool KeepJobFlowaliveWhenNoSteps { get; set; }
        public string MasterInstanceType { get; set; }
        public string SlaveInstanceType { get; set; }
        public string AvailabilityZone { get; set; }
        public string LogURI { get; set; }
        public string JobFlowName { get; set; }
        public Amazon.ElasticMapReduce.ActionOnFailure ActionOnFailure { get; set; }
        public string MainClass { get; set; }
        public string Jar { get; set; }
        public string stepName { get; set; }
        public List<string> Args { get; set; }

        public static EMRDeveloperOptions Parse(string devOptions)
        {
            EMRDeveloperOptions options = new EMRDeveloperOptions();
            String lastKey = "";

            if (!string.IsNullOrWhiteSpace(devOptions))
            {
                // splitting all entries into arrays of optionPairs
                var optionPairs = devOptions.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var optionPair in optionPairs)
                {
                    // splitting all optionPairs into arrays of key/value denominations
                    var optionPairParts = optionPair.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    if (optionPairParts.Length == 2)
                    {
                        if (lastKey.Equals(optionPairParts[0].Trim().ToLowerInvariant()))
                        {
                            MapToOptionWithCollection(options, optionPairParts[0].Trim().ToLowerInvariant(), optionPairParts[1].Trim(), lastKey);
                        }
                        else
                        {
                            MapToOption(options, optionPairParts[0].Trim().ToLowerInvariant(), optionPairParts[1].Trim());
                            lastKey = optionPairParts[0].Trim().ToLowerInvariant();
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

        // Use this method to map all collections to their proper places.
        // Usage here is to confirm that the subsequent key is the same as the preceding key. 
        // This forces that the REST call ensure all collection parameters are grouped together.
        // Ex. This is good: (key1=value&key1=value2)
        // Ex. This is bad: (key1=value&key2=value2)
        // Ex. This is bad: (key1=value&key2=value2&key1=value3)
        public static void MapToOptionWithCollection(EMRDeveloperOptions existingOptions, string key, string value, string lastKey)
        {
            if (key.Equals(lastKey))
            {
                if ("args".Equals(key))
                {
                    existingOptions.Args.Add(value);
                    return;
                }
                throw new ArgumentException(string.Format("The developer option '{0}' was not expected and is not understood.", key));
            }
            throw new ArgumentException(string.Format("The developer option '{0}' is grouped out of order in the REST call. Group collection parameters together in the request.", key));
        }

        // Interior method takes in instance of DeveloperOptions (aptly named existingOptions) and maps them to the proper value. In essence, a setter method.
        public static void MapToOption(EMRDeveloperOptions existingOptions, string key, string value)
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

            if ("availabilityzone".Equals(key))
            {
                existingOptions.AvailabilityZone = value;
                return;
            }

            if ("actiononfailure".Equals(key))
            {
                existingOptions.ActionOnFailure = new Amazon.ElasticMapReduce.ActionOnFailure(value);
                return;
            }

            if ("ec2keyname".Equals(key))
            {
                existingOptions.Ec2KeyName = value;
                return;
            }

            if ("instancecount".Equals(key))
            {
                int result;
                if (!int.TryParse(value, out result))
                {
                    throw new ArgumentException(string.Format("The developer option '{0}' can only have an integer value but '{1}' was used instead.", key, value));
                }
                existingOptions.InstanceCount = result;
                return;
            }

            if ("jar".Equals(key))
            {
                existingOptions.Jar = value;
                return;
            }

            if ("jobflowname".Equals(key))
            {
                existingOptions.JobFlowName = value;
                return;
            }

            if ("args".Equals(key))
            {
                existingOptions.Args.Add(value);
                return;
            }

            if ("loguri".Equals(key))
            {
                existingOptions.LogURI = value;
                return;
            }

            if ("mainclass".Equals(key))
            {
                existingOptions.MainClass = value;
                return;
            }

            if ("masterinstancetype".Equals(key))
            {
                existingOptions.MasterInstanceType = value;
                return;
            }

            if ("slaveinstancetype".Equals(key))
            {
                existingOptions.SlaveInstanceType = value;
                return;
            }

            if ("stepname".Equals(key))
            {
                existingOptions.stepName = value;
                return;
            }

            if ("keepjobflowalivewhennosteps".Equals(key))
            {
                bool result;
                if (!bool.TryParse(value, out result))
                {
                    throw new ArgumentException(string.Format("The developer option '{0}' must be a boolean value.", key));
                }
                existingOptions.KeepJobFlowaliveWhenNoSteps = result;
                return;
            }

            if ("secretaccesskey".Equals(key))
            {
                existingOptions.SecretAccessKey = value;
                return;
            }
            throw new ArgumentException(string.Format("The developer option '{0}' was not expected and is not understood.", key));
        }
    }
}
