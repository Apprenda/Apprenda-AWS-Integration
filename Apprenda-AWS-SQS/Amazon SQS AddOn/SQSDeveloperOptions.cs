using System;
using System.Collections.Generic;

namespace Apprenda.SaaSGrid.Addons.AWS.SQS
{
    public class SQSAttributes
    {
        public int DelaySeconds { get; set; }
        public int MaximumMessageSize { get; set; }
        public int MessageRetentionPeriod { get; set; }

        public Dictionary<string, string> ToDict()
        {
            throw new NotImplementedException();
        }
    }

    public class SQSDeveloperOptions
    {
        // Amazon Credentials. Required for IAM. 
        public string AccessKey { get; private set; }
        public string SecretAccessKey { get; private set; }
        public string QueueName { get; private set; }
        public SQSAttributes Attributes { get; private set; }
        
        public static SQSDeveloperOptions Parse(IEnumerable<AddonParameter> _developerParameters)
        {
            var options = new SQSDeveloperOptions() { Attributes = new SQSAttributes() };
            foreach (var parameter in _developerParameters)
            {
                MapToOption(options, parameter.Key.ToLowerInvariant(), parameter.Value);
            }
            return options;
        }

        // Interior method takes in instance of DeveloperOptions (aptly named existingOptions) and maps them to the proper value. In essence, a setter method.
        private static void MapToOption(SQSDeveloperOptions _existingOptions, string _key, string _value)
        {
            if ("accesskey".Equals(_key))
            {
                _existingOptions.AccessKey = _value;
                return;
            }
            if ("secretkey".Equals(_key))
            {
                _existingOptions.SecretAccessKey = _value;
                return;
            }
            if ("queuename".Equals(_key))
            {
                _existingOptions.QueueName = _value;
                return;
            }
            if ("delayseconds".Equals(_key))
            {
                int tmp;
                if(int.TryParse(_value, out tmp))
                {
                    _existingOptions.Attributes.DelaySeconds = tmp;
                }
                else
                {
                    throw new ArgumentException(string.Format("Unable to parse the developer option '{0}'.", _key));
                }
                return;
            }
            if ("maximummessagesize".Equals(_key))
            {
                int tmp;
                if (int.TryParse(_value, out tmp))
                {
                    _existingOptions.Attributes.MaximumMessageSize = tmp;
                }
                else
                {
                    throw new ArgumentException(string.Format("Unable to parse the developer option '{0}'.", _key));
                }
                return;
            } 
            if ("maximumretentionperiod".Equals(_key))
            {
                int tmp;
                if (int.TryParse(_value, out tmp))
                {
                    _existingOptions.Attributes.MessageRetentionPeriod = tmp;
                }
                else
                {
                    throw new ArgumentException(string.Format("Unable to parse the developer option '{0}'.", _key));
                }
                return;
            }
            throw new ArgumentException(string.Format("The developer option '{0}' was not expected and is not understood.", _key));
        }     
    }
}