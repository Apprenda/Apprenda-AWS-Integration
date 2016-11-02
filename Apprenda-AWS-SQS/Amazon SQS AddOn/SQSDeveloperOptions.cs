using System;
using System.Collections.Generic;

namespace Apprenda.SaaSGrid.Addons.AWS.SQS
{
    public class SQSAttributes
    {
        private static uint DEFAULT_DELAY_SECONDS = 0;
        public static uint DEFAULT_MAXIMUM_MESSAGE_SIZE = 262144;
        private static uint DEFAULT_MESSAGE_RETENTION_PERIOD = 345600;
        public static uint MAX_DELAY_SECONDS = 900;
        public static uint MAX_RETENTION_PERIOD = 1209600;
        public static uint MIN_MESSAGE_SIZE = 1024;
        public static uint MIN_RETENTION_PERIOD = 60;

        public uint DelaySeconds { get; set; }
        public uint MaximumMessageSize { get; set; }
        public uint MessageRetentionPeriod { get; set; }

        public SQSAttributes()
        {
            this.DelaySeconds = DEFAULT_DELAY_SECONDS;
            this.MaximumMessageSize = DEFAULT_MAXIMUM_MESSAGE_SIZE;
            this.MessageRetentionPeriod = DEFAULT_MESSAGE_RETENTION_PERIOD;
        }


        public Dictionary<string, string> ToDict()
        {
            return new Dictionary<string, string>
            {
                { "DelaySeconds", this.DelaySeconds.ToString() },
                { "MaximumMessageSize", this.MaximumMessageSize.ToString() },
                {
                    "MessageRetentionPeriod",
                    this.MessageRetentionPeriod.ToString()
                }
            };
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
                uint tmp;
                if(uint.TryParse(_value, out tmp))
                {
                    if (tmp > SQSAttributes.MAX_DELAY_SECONDS)
                    {
                        throw new ArgumentOutOfRangeException("Delayseconds must be between 0 and 900");
                    }
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
                uint tmp;
                if (uint.TryParse(_value, out tmp))
                {
                    if (tmp < SQSAttributes.MIN_MESSAGE_SIZE || tmp < SQSAttributes.DEFAULT_MAXIMUM_MESSAGE_SIZE)
                    {
                        throw new ArgumentOutOfRangeException("MaximumMessageSize must be between 1024 and 262144");
                    }
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
                uint tmp;
                if (uint.TryParse(_value, out tmp))
                {
                    if (tmp < SQSAttributes.MIN_RETENTION_PERIOD || tmp > SQSAttributes.MAX_RETENTION_PERIOD)
                    {
                        throw new ArgumentOutOfRangeException("MessageRetentionPeriod must be between 60 and 1209600");
                    }
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