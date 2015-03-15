using System;
using System.Collections.Generic;

namespace Apprenda.SaaSGrid.Addons.AWS.EMR
{
    class EMRDeveloperOptions
    {
        public string Ec2KeyName { get; private set; }
        public int InstanceCount { get; private set; }
        public bool KeepJobFlowaliveWhenNoSteps { get; private set; }
        public string MasterInstanceType { get; private set; }
        public string SlaveInstanceType { get; private set; }
        public string AvailabilityZone { get; private set; }
        public string LogUri { get; private set; }
        public string JobFlowName { get; private set; }
        public Amazon.ElasticMapReduce.ActionOnFailure ActionOnFailure { get; private set; }
        public string MainClass { get; private set; }
        public string Jar { get; private set; }
        public string StepName { get; private set; }
        public List<string> Args { get; private set; }
        public bool EnableDebugging { get; private set; }

        public static EMRDeveloperOptions Parse(IEnumerable<AddonParameter> devParameters)
        {
            var options = new EMRDeveloperOptions();

            foreach (var parameter in devParameters)
            {
                MapToOption(options, parameter.Key.ToLowerInvariant(), parameter.Value);
            }
            return options;
        }

        // Interior method takes in instance of DeveloperOptions (aptly named existingOptions) and maps them to the proper value. In essence, a setter method.
        private static void MapToOption(EMRDeveloperOptions existingOptions, string key, string value)
        {
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

            if("enabledebugging".Equals(key))
            {
                bool result;
                if(bool.TryParse(value, out result))
                {
                    throw new ArgumentException(string.Format("The developer option '{0}' can only have an integer value but '{1}' was used instead.", key, value));
                }
                existingOptions.EnableDebugging = result;
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
                existingOptions.LogUri = value;
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
                existingOptions.StepName = value;
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
        }

        
    }
}
