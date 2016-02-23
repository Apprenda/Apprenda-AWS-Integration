using System;

namespace Apprenda.SaaSGrid.Addons.AWS.EC2
{
    public class DeveloperOptions
    {
        // for now phase I will be external. i'm not dealing with VPC just yet.

        internal bool UseClientRegion { get; private set; }

        internal String RegionEndpont { get; private set; }

        internal string AccessKey { get; private set; }

        internal string SecretAccessKey { get; private set; }

        internal string AmiId { get; private set; }

        internal string Ec2KeyPair { get; private set; }

        internal string SecurityGroupId { get; private set; }

        internal string InstanceType { get; private set; }

        internal string InstanceCommonName { get; private set; }

        public static DeveloperOptions Parse(string devOptions)
        {
            var options = new DeveloperOptions();

            if (string.IsNullOrWhiteSpace(devOptions)) return options;
            var optionPairs = devOptions.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
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
            return options;
        }

        private static void MapToOption(DeveloperOptions existingOptions, string key, string value)
        {
            if (key.Equals("amiid"))
            {
                existingOptions.AmiId = value;
                return;
            }
            if (key.Equals("instancetype"))
            {
                existingOptions.InstanceType = value;
                return;
            }
            // else option is not found, throw exception
            throw new ArgumentException(
                string.Format("The developer option '{0}' was not expected and is not understood.", key));
        }

        internal void LoadItemsFromManifest(AddonManifest manifest)
        {
            try
            {
                var manifestProperties = manifest.GetProperties();
                foreach (var manifestProperty in manifestProperties)
                {
                    //Console.WriteLine("Debug- manifestProperty Key: " + manifestProperty.DisplayName + " Value: " + manifestProperty.Value);
                    switch (manifestProperty.Key.Trim().ToLowerInvariant())
                    {
                        case ("acesskey"):
                            AccessKey = manifestProperty.Value;
                            break;

                        case ("secretkey"):
                            SecretAccessKey = manifestProperty.Value;
                            break;

                        case ("useclientregion"):
                            bool tmp;
                            bool.TryParse(manifestProperty.Value, out tmp);
                            UseClientRegion = tmp;
                            break;

                        case ("regionEndpoint"):
                            RegionEndpont = manifestProperty.Value;
                            break;

                        case ("amiid"):
                            AmiId = manifestProperty.Value;
                            break;

                        case ("ec2keypair"):
                            Ec2KeyPair = manifestProperty.Value;
                            break;

                        case ("securityGroups"):
                            SecurityGroupId = manifestProperty.Value;
                            break;

                        case ("defaultinstancetype"):
                            InstanceType = manifestProperty.Value;
                            break;

                        case ("instancecommonname"):
                            InstanceCommonName = manifestProperty.Value;
                            break;

                        default: // means there are other manifest properties we don't need.
                            Console.WriteLine("Key not added: " + manifestProperty.DisplayName);
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message + "\n Debug information: " + manifest.GetProperties());
            }
        }
    }
}