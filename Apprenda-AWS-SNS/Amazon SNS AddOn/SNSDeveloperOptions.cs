using System;
using System.Collections.Generic;

namespace Apprenda.SaaSGrid.Addons.AWS.SNS
{
    public class SnsDeveloperOptions
    {
        // Amazon Credentials. Required for IAM. 
        public string TopicName { get; set; }

        public string PlatformApplicationName { get; set; }

        // we'll handle this in 1.1. just topic creation for now.
        //public Dictionary<string, string> PlatformAttributes { get; set; }

        //public string MessagingPlatform { get; set; }

        //public string PlatformApplicationArn { get; set; }

        //public string CustomUserData { get; set; }

        //public string Token { get; set; }

        //public Dictionary<string, string> EndpointAttributes { get; set; }

        // Method takes in a string and parses it into a DeveloperOptions class.
        public static SnsDeveloperOptions Parse(IEnumerable<AddonParameter> _developerParameters, AddonManifest _manifest)
        {
            var options = new SnsDeveloperOptions();
            foreach (var parameter in _developerParameters)
            {
                MapToOption(options, parameter.Key.ToLowerInvariant(), parameter.Value);
            }
            foreach (var parameter in _manifest.Properties)
            {
                MapToOption(options, parameter.Key.ToLowerInvariant(), parameter.Value);    
            }
            return options;
        }

        // Interior method takes in instance of DeveloperOptions (aptly named existingOptions) and maps them to the proper value. In essence, a setter method.
        private static void MapToOption(SnsDeveloperOptions _existingOptions, string _key, string _value)
        {
            if("topicname".Equals(_key))
            {
                _existingOptions.TopicName = _value;
                return;
            }
            /*
            if ("platformapplicationname".Equals(_key))
            {
                _existingOptions.PlatformApplicationName = _value;
                return;
            }
            if ("platformapplicationarn".Equals(_key))
            {
                _existingOptions.PlatformApplicationArn = _value;
                return;
            }
            if ("existingoptions".Equals(_key))
            {
                _existingOptions.Token = _value;
                return;
            }
            if ("customuserdata".Equals(_key))
            {
                _existingOptions.CustomUserData = _value;
                return;
            }
            */
            throw new ArgumentException(string.Format("The developer option '{0}' was not expected and is not understood.", _key));
        }

        
    }
}