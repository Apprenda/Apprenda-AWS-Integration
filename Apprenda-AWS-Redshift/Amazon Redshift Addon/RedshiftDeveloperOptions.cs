using System;
using System.Collections.Generic;

namespace Apprenda.SaaSGrid.Addons.AWS.Redshift
{
    public class RedshiftDeveloperOptions
    {
        public string ClusterParameterGroupName { get; set; }
        public string MasterUserName { get; set; }
        public string MasterPassword { get; set; }

        public string AccessKey { get; set; }
        public string SecretAccessKey { get; set; }

        public string RegionEndpoint { get; set; }
        // Amazon Redshift Options
        public int? AllocatedStorage { get; set; }
        public string AvailabilityZone { get; set; }
        public bool AllowVersionUpgrade { get; set; }
        public int AutomatedSnapshotRetentionPeriod { get; set; }
        public string ClusterIdentifier { get; set; }
        public List<string> ClusterSecurityGroups { get; set; }
        public string ClusterSubnetGroupName { get; set; }
        public string ClusterType { get; set; }
        public string ClusterVersion { get; set; }
        public string DBName { get; set; }
        public string ElasticIp { get; set; }
        public bool Encrypted { get; set; }
        public string HSMClientCertificateIdentifier { get; set; }
        public string HSMClientConfigurationIdentifier { get; set; }
        public string NodeType { get; set; }
        public int NumberOfNodes { get; set; }
        public int Port { get; set; }
        public string PreferredMaintenanceWindow { get; set; }
        public bool PubliclyAccessible { get; set; }
        public List<string> VpcSecurityGroupIds { get; set; }

        // Method takes in a string and parses it into a DeveloperOptions class.
        public static RedshiftDeveloperOptions Parse(string developerOptions)
            {
                // modified to include a list representaiton from parameter arguments
                // so! how it works is as follows: 
                // http://<url>/path/to/rest/call?list=item1&list=item2&list=item3

                RedshiftDeveloperOptions options = new RedshiftDeveloperOptions();
                String lastKey = "";

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
        public static void MapToOptionWithCollection(RedshiftDeveloperOptions existingOptions, string key, string value, string lastKey)
            {
                if (key.Equals(lastKey))
                {
                    if ("clustersecuritygroups".Equals(key))
                    {
                        existingOptions.ClusterSecurityGroups.Add(value);
                        return;
                    }
                    if ("vpcsecuritygroupids".Equals(key))
                    {
                        existingOptions.VpcSecurityGroupIds.Add(value);
                        return;
                    }
                    throw new ArgumentException(string.Format("The developer option '{0}' was not expected and is not understood.", key));
                }
                throw new ArgumentException(string.Format("The developer option '{0}' is grouped out of order in the REST call. Group collection parameters together in the request.", key));
            }

            // Interior method takes in instance of DeveloperOptions (aptly named existingOptions) and maps them to the proper value. In essence, a setter method.
        public static void MapToOption(RedshiftDeveloperOptions existingOptions, string key, string value)
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

                if("regionendpoint".Equals(key))
                {
                    existingOptions.RegionEndpoint = value;
                }

                if ("availabilityzone".Equals(key))
                {
                    existingOptions.AvailabilityZone = value;
                    return;
                }

                if ("allocatedstorage".Equals(key))
                {
                    int result;
                    if (!int.TryParse(value, out result))
                    {
                        throw new ArgumentException(string.Format("The developer option '{0}' can only have an integer value but '{1}' was used instead.", key, value));
                    }
                    existingOptions.AllocatedStorage = result;
                    return;
                }

                if ("allowversionupgrade".Equals(key))
                {
                    bool result;
                    if (!bool.TryParse(value, out result))
                    {
                        throw new ArgumentException(string.Format("The developer option '{0}' must be a boolean value.", key));
                    }
                    existingOptions.AllowVersionUpgrade = result;
                    return;
                }

                if ("automatedsnapshotretentionperiod".Equals(key))
                {
                    int result;
                    if(!int.TryParse(value, out result))
                    {
                        throw new ArgumentException(string.Format("The developer option '{0}' can only have an integer value but '{1}' was used instead.", key, value));
                    }
                    existingOptions.AutomatedSnapshotRetentionPeriod = result;
                    return;
                }

                if ("clusteridentifier".Equals(key))
                {
                    existingOptions.ClusterIdentifier = value;
                    return;
                }

                if ("clusterparametergroupname".Equals(key))
                {
                    existingOptions.ClusterParameterGroupName = value;
                    return;
                }

                if ("clustersecuritygroups".Equals(key))
                {
                    if(true)
                    {
                    existingOptions.ClusterSecurityGroups.Add(value);
                    }
                    return;
                }

                if ("clustersubnetgroupname".Equals(key))
                {
                    existingOptions.ClusterSubnetGroupName = value;
                    return;
                }

                if ("clustertype".Equals(key))
                {
                    existingOptions.ClusterType = value;
                    return;
                }

                if ("clusterversion".Equals(key))
                {
                    existingOptions.ClusterVersion = value;
                    return;
                }

                if ("dbname".Equals(key))
                {
                    existingOptions.DBName = value;
                    return;
                }

                if ("elasticip".Equals(key))
                {
                    existingOptions.ElasticIp = value;
                    return;
                }

                if ("encrypted".Equals(key))
                {
                    bool result;
                    if(!bool.TryParse(value, out result))
                    {
                        throw new ArgumentException(string.Format("The developer option '{0}' must be a boolean value.", key));
                    }
                    existingOptions.Encrypted = result;
                    return;
                }

                if ("hsmclientcertificateidentifier".Equals(key))
                {
                    existingOptions.HSMClientCertificateIdentifier = value;
                    return;
                }

                if ("hsmclientconfigurationidentifier".Equals(key))
                {
                    existingOptions.HSMClientConfigurationIdentifier = value;
                    return;
                }

                if ("masterpassword".Equals(key))
                {
                    existingOptions.MasterPassword = value;
                    return;
                }

                if ("masterusername".Equals(key))
                {
                    existingOptions.MasterUserName = value;
                    return;
                }

                if ("nodetype".Equals(key))
                {
                    existingOptions.NodeType = value;
                    return;
                }

                if ("numberofnodes".Equals(key))
                {
                    int result;
                    if (!int.TryParse(value, out result))
                    {
                        throw new ArgumentException(string.Format("The developer option '{0}' must be a boolean value.", key));
                    }
                    existingOptions.NumberOfNodes = result;
                    return;
                }

                if ("port".Equals(key))
                {
                    int result;
                    if (!int.TryParse(value, out result))
                    {
                        throw new ArgumentException(string.Format("The developer option '{0}' must be a boolean value.", key));
                    }
                    existingOptions.Port = result;
                    return;
                }

                if ("preferredmaintenancewindow".Equals(key))
                {
                    existingOptions.PreferredMaintenanceWindow = value;
                    return;
                }

                if ("publiclyaccessible".Equals(key))
                {
                    bool result;
                    if (!bool.TryParse(value, out result))
                    {
                        throw new ArgumentException(string.Format("The developer option '{0}' must be a boolean value.", key));
                    }
                    existingOptions.PubliclyAccessible = result;
                    return;
                }

                if ("secretaccesskey".Equals(key))
                {
                    existingOptions.SecretAccessKey = value;
                    return;
                } 
                
                if ("vpcsecuritygroupids".Equals(key))
                {
                    existingOptions.VpcSecurityGroupIds.Add(value);
                    return;
                } 
                throw new ArgumentException(string.Format("The developer option '{0}' was not expected and is not understood.", key));
            }
        }
    }
