using Amazon;
using Amazon.Redshift;
using Amazon.Redshift.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Apprenda.SaaSGrid.Addons.AWS.Redshift
{
    public class RedhsiftAddOn : AddonBase
    {
        // Deprovision Redshift Instance
        // Input: AddonDeprovisionRequest request
        // Output: OperationResult
        public override OperationResult Deprovision(AddonDeprovisionRequest request)
        {
            var connectionData = request.ConnectionData;
            var deprovisionResult = new ProvisionAddOnResult(connectionData);
            var manifest = request.Manifest;
            var devParams = request.DeveloperParameters;

            try
            {
                AmazonRedshiftClient client;
                var conInfo = RedshiftConnectionInfo.Parse(connectionData);
                var developerOptions = RedshiftDeveloperOptions.Parse(devParams);
                var establishClientResult = EstablishClient(manifest, developerOptions, out client);
                if (!establishClientResult.IsSuccess)
                {
                    deprovisionResult.EndUserMessage = establishClientResult.EndUserMessage;
                    return deprovisionResult;
                }
                var response =
                    client.DeleteCluster(new DeleteClusterRequest
                    {
                        ClusterIdentifier = conInfo.ClusterIdentifier,
                    });
                if (response.Cluster != null)
                {
                    do
                    {
                        var verificationResponse = client.DescribeClusters(new DescribeClustersRequest
                        {
                            ClusterIdentifier = conInfo.ClusterIdentifier
                        });
                        if (!verificationResponse.Clusters.Any())
                        {
                            deprovisionResult.IsSuccess = true;
                            break;
                        }
                        Thread.Sleep(TimeSpan.FromSeconds(10d));
                    } while (true);
                }
            }
            catch (ClusterNotFoundException)
            {
                deprovisionResult.IsSuccess = true;
            }
            catch (Exception e)
            {
                deprovisionResult.EndUserMessage = e.Message;
            }

            return deprovisionResult;
        }

        // Provision Redshift Instance
        // Input: AddonDeprovisionRequest request
        // Output: ProvisionAddOnResult
        public override ProvisionAddOnResult Provision(AddonProvisionRequest request)
        {
            var provisionResult = new ProvisionAddOnResult("") { IsSuccess = false };
            var manifest = request.Manifest;
            var developerParameters = request.DeveloperParameters;

            try
            {
                AmazonRedshiftClient client;
                RedshiftDeveloperOptions devOptions;
                var parseOptionsResult = ParseDevOptions(developerParameters, out devOptions);
                if (!parseOptionsResult.IsSuccess)
                {
                    provisionResult.EndUserMessage = parseOptionsResult.EndUserMessage;
                    return provisionResult;
                }
                var establishClientResult = EstablishClient(manifest, devOptions, out client);
                if (!establishClientResult.IsSuccess)
                {
                    provisionResult.EndUserMessage = establishClientResult.EndUserMessage;
                    return provisionResult;
                }

                var response = client.CreateCluster(CreateClusterRequest(devOptions));
                if (response.Cluster != null)
                {
                    do
                    {
                        var verificationResponse = client.DescribeClusters(new DescribeClustersRequest
                        {
                            ClusterIdentifier = devOptions.ClusterIdentifier
                        });
                        if (verificationResponse.Clusters.Any() && verificationResponse.Clusters[0].ClusterStatus == "available")
                        {
                            var dbInstance = verificationResponse.Clusters[0];
                            var conInfo = new RedshiftConnectionInfo
                            {
                                ClusterIdentifier = devOptions.ClusterIdentifier,
                                EndpointAddress = dbInstance.Endpoint.Address,
                                EndpointPort = dbInstance.Endpoint.Port
                            };
                            provisionResult.IsSuccess = true;
                            provisionResult.ConnectionData = conInfo.ToString();
                            break;
                        }
                        Thread.Sleep(TimeSpan.FromSeconds(10d));
                    } while (true);
                }
            }
            catch (Exception e)
            {
                provisionResult.EndUserMessage = e.Message;
            }

            return provisionResult;
        }

        // Testing Instance
        // Input: AddonTestRequest request
        // Output: OperationResult
        public override OperationResult Test(AddonTestRequest request)
        {
            var manifest = request.Manifest;
            var developerParameters = request.DeveloperParameters;
            var testResult = new OperationResult { IsSuccess = false };
            var testProgress = "";
            if (manifest.Properties != null && manifest.Properties.Any())
            {
                RedshiftDeveloperOptions devOptions;

                testProgress += "Evaluating required manifest properties...\n";
                if (!ValidateManifest(manifest, out testResult))
                {
                    return testResult;
                }

                var parseOptionsResult = ParseDevOptions(developerParameters, out devOptions);
                if (!parseOptionsResult.IsSuccess)
                {
                    return parseOptionsResult;
                }
                testProgress += parseOptionsResult.EndUserMessage;

                try
                {
                    testProgress += "Establishing connection to AWS...\n";
                    AmazonRedshiftClient client;
                    var establishClientResult = EstablishClient(manifest, devOptions, out client);
                    if (!establishClientResult.IsSuccess)
                    {
                        return establishClientResult;
                    }
                    testProgress += establishClientResult.EndUserMessage;

                    client.DescribeClusters();
                    testProgress += "Successfully passed all testing criteria!";
                    testResult.IsSuccess = true;
                    testResult.EndUserMessage = testProgress;
                }
                catch (Exception e)
                {
                    testResult.EndUserMessage = e.Message;
                }
            }
            else
            {
                testResult.EndUserMessage = "Missing required manifest properties (requireDevCredentials)";
            }

            return testResult;
        }

        /* Begin private methods */

        private static bool ValidateDevCreds(RedshiftDeveloperOptions devOptions)
        {
            return !(string.IsNullOrWhiteSpace(devOptions.AccessKey) || string.IsNullOrWhiteSpace(devOptions.SecretAccessKey));
        }

        private static OperationResult ParseDevOptions(IEnumerable<AddonParameter> developerParameters, out RedshiftDeveloperOptions devOptions)
        {
            devOptions = null;
            var result = new OperationResult { IsSuccess = false };
            var progress = "";

            try
            {
                progress += "Parsing developer options...\n";
                devOptions = RedshiftDeveloperOptions.Parse(developerParameters);
            }
            catch (ArgumentException e)
            {
                result.EndUserMessage = e.Message;
                return result;
            }

            result.IsSuccess = true;
            result.EndUserMessage = progress;
            return result;
        }

        private static OperationResult EstablishClient(AddonManifest manifest, RedshiftDeveloperOptions devOptions, out AmazonRedshiftClient client)
        {
            OperationResult result;

            bool requireCreds;
            var manifestprops = manifest.GetProperties().ToDictionary(x => x.Key, x => x.Value);
            var accessKey = manifestprops["AWSClientKey"];
            var secretAccessKey = manifestprops["AWSSecretKey"];
            //var AWSRegionEndpoint = manifestprops["AWSRegionEndpoint"];

            var prop =
                manifest.Properties.First(
                    p => p.Key.Equals("requireDevCredentials", StringComparison.InvariantCultureIgnoreCase));

            if (bool.TryParse(prop.Value, out requireCreds) && requireCreds)
            {
                if (!ValidateDevCreds(devOptions))
                {
                    client = null;
                    result = new OperationResult
                    {
                        IsSuccess = false,
                        EndUserMessage =
                            "The add on requires that developer credentials are specified but none were provided."
                    };
                    return result;
                }
            }
            var config = new AmazonRedshiftConfig { RegionEndpoint = RegionEndpoint.USEast1 };
            client = new AmazonRedshiftClient(accessKey, secretAccessKey, config);
            result = new OperationResult { IsSuccess = true };
            return result;
        }

        private static CreateClusterRequest CreateClusterRequest(RedshiftDeveloperOptions devOptions)
        {
            return new CreateClusterRequest
            {
                AllowVersionUpgrade = devOptions.AllowVersionUpgrade,
                AutomatedSnapshotRetentionPeriod = devOptions.AutomatedSnapshotRetentionPeriod,
                AvailabilityZone = devOptions.AvailabilityZone,
                ClusterIdentifier = devOptions.ClusterIdentifier,
                ClusterParameterGroupName = devOptions.ClusterParameterGroupName,
                ClusterSecurityGroups = devOptions.ClusterSecurityGroups,
                ClusterSubnetGroupName = devOptions.ClusterSubnetGroupName,
                ClusterType = devOptions.ClusterType,
                ClusterVersion = devOptions.ClusterVersion,
                DBName = devOptions.DbName,
                ElasticIp = devOptions.ElasticIp,
                Encrypted = devOptions.Encrypted,
                HsmClientCertificateIdentifier = devOptions.HsmClientCertificateIdentifier,
                HsmConfigurationIdentifier = devOptions.HsmClientConfigurationIdentifier,
                MasterUsername = devOptions.MasterUserName,
                MasterUserPassword = devOptions.MasterPassword,
                NodeType = devOptions.NodeType,
                NumberOfNodes = devOptions.NumberOfNodes,
                Port = devOptions.Port,
                PreferredMaintenanceWindow = devOptions.PreferredMaintenanceWindow,
                PubliclyAccessible = devOptions.PubliclyAccessible,
                VpcSecurityGroupIds = devOptions.VpcSecurityGroupIds
            };
        }

        private static bool ValidateManifest(AddonManifest manifest, out OperationResult testResult)
        {
            testResult = new OperationResult();

            var prop =
                    manifest.Properties.FirstOrDefault(
                        p => p.Key.Equals("requireDevCredentials", StringComparison.InvariantCultureIgnoreCase));

            if (prop == null || !prop.HasValue)
            {
                testResult.IsSuccess = false;
                testResult.EndUserMessage = "Missing required property 'requireDevCredentials'. This property needs to be provided as part of the manifest";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(manifest.ProvisioningUsername) &&
                !string.IsNullOrWhiteSpace(manifest.ProvisioningPassword)) return true;
            testResult.IsSuccess = false;
            testResult.EndUserMessage = "Missing credentials 'provisioningUsername' & 'provisioningPassword' . These values needs to be provided as part of the manifest";
            return false;
        }
    }
}