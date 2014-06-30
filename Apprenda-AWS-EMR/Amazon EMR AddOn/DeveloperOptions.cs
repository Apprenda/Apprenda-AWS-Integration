using System;


namespace AWS_EMR_AddOn
{
    public class DeveloperOptions
    {
        public string AccessKey { get; set; }
        public string SecretKey { get; set; }
        public string AvailabilityZone { get; set; }
        //public string JobFlowId { get; set; }

        public static DeveloperOptions Parse(string developerOptions)
        {
            DeveloperOptions options = new DeveloperOptions();

            if (!string.IsNullOrWhiteSpace(developerOptions))
            {
                var optionPairs = developerOptions.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var optionPair in optionPairs)
                {
                    var optionPairParts = optionPair.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    if (optionPairParts.Length == 2)
                    {
                        MapToOption(options, optionPairParts[0].Trim().ToLowerInvariant(), optionPairParts[1].Trim());
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

        private static void MapToOption(DeveloperOptions existingOptions, string key, string value)
        {
            if ("accesskey".Equals(key))
            {
                existingOptions.AccessKey = value;
                return;
            }

            if ("secretkey".Equals(key))
            {
                existingOptions.SecretKey = value;
                return;
            }

            /*if ("availabilityzone".Equals(key))
            {
                existingOptions.AvailabilityZone = value;
                return;
            }*/

            throw new ArgumentException(string.Format("The developer option '{0}' was not expected and is not understood.", key));
        }
    }
}
