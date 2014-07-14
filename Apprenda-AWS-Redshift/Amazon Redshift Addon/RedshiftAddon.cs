using Amazon.Redshift.Model;
using Apprenda.SaaSGrid.Addons;
using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Redshift;
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
            string connectionData = request.ConnectionData;
            var deprovisionResult = new ProvisionAddOnResult(connectionData);
            AddonManifest manifest = request.Manifest;
            
            string devOptions = request.DeveloperOptions;

            try
            {
                AmazonRedshiftClient client;
                var conInfo = ConnectionInfo.Parse(connectionData);
                var developerOptions = RedshiftDeveloperOptions.Parse(devOptions);

                var establishClientResult = EstablishClient(manifest, developerOptions, out client);
                if (!establishClientResult.IsSuccess)
                {
                    deprovisionResult.EndUserMessage = establishClientResult.EndUserMessage;
                    return deprovisionResult;
                }

                var response =
                    client.DeleteCluster(new DeleteClusterRequest()
                    {
                        ClusterIdentifier = conInfo.ClusterIdentifier,
                        //SkipFinalSnapshot = true
                    });
                // modified 5/22/14 to fix deprecation in Amazon AWS SDK
                if (response.Cluster != null)
                {
                    do
                    {
                        var verificationResponse = client.DescribeClusters(new DescribeClustersRequest()
                        {
                            ClusterIdentifier = conInfo.ClusterIdentifier
                        });
                        // modified 5/22/14 to fix deprecation in Amazon AWS SDK
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
            AddonManifest manifest = request.Manifest;
            string developerOptions = request.DeveloperOptions;

            try
            {
                AmazonRedshiftClient client;
                RedshiftDeveloperOptions devOptions;

                var parseOptionsResult = ParseDevOptions(developerOptions, out devOptions);
                if (!parseOptionsResult.IsSuccess)
                {
                    provisionResult.EndUserMessage = parseOptionsResult.EndUserMessage;
                    return provisionResult;
                }

                var establishClientResult = EstablishClient(manifest, RedshiftDeveloperOptions.Parse(developerOptions), out client);
                if (!establishClientResult.IsSuccess)
                {
                    provisionResult.EndUserMessage = establishClientResult.EndUserMessage;
                    return provisionResult;
                }

                var response = client.CreateCluster(CreateClusterRequest(devOptions));
                // modified 5/22/14 to fix amazon aws deprecation
                if (response.Cluster != null)
                {
                    //var conInfo = new ConnectionInfo()
                    //{
                    //    DbInstanceIdentifier = devOptions.DbInstanceIndentifier
                    //};
                    //provisionResult.IsSuccess = true;
                    //provisionResult.ConnectionData = conInfo.ToString();
                    //Thread.Sleep(TimeSpan.FromMinutes(6));

                    do
                    {
                        var verificationResponse = client.DescribeClusters(new DescribeClustersRequest()
                        {
                            ClusterIdentifier = devOptions.ClusterIdentifier
                        });
                        // next few lines fixed 5/22/14 to resolve amazon aws deprecation
                        if (verificationResponse.Clusters.Any() && verificationResponse.Clusters[0].ClusterStatus == "available")
                        {
                            var dbInstance = verificationResponse.Clusters[0];
                            var conInfo = new ConnectionInfo()
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
            AddonManifest manifest = request.Manifest;
            string developerOptions = request.DeveloperOptions;
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

                var parseOptionsResult = ParseDevOptions(developerOptions, out devOptions);
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

        

        // TODO: We might be able to extend this. 
        private bool ValidateDevCreds(RedshiftDeveloperOptions devOptions)
        {
            return !(string.IsNullOrWhiteSpace(devOptions.AccessKey) || string.IsNullOrWhiteSpace(devOptions.SecretAccessKey));
        }

        private OperationResult ParseDevOptions(string developerOptions, out RedshiftDeveloperOptions devOptions)
        {
            devOptions = null;
            var result = new OperationResult() { IsSuccess = false };
            var progress = "";

            try
            {
                progress += "Parsing developer options...\n";
                devOptions = RedshiftDeveloperOptions.Parse(developerOptions);
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

        private OperationResult EstablishClient(AddonManifest manifest, RedshiftDeveloperOptions devOptions, out AmazonRedshiftClient client)
        {
            OperationResult result;

            bool requireCreds;
            var accessKey = manifest.ProvisioningUsername;
            var secretAccessKey = manifest.ProvisioningPassword;

            var prop =
                manifest.Properties.First(
                    p => p.Key.Equals("requireDevCredentials", StringComparison.InvariantCultureIgnoreCase));

            if (bool.TryParse(prop.Value, out requireCreds) && requireCreds)
            {
                if (!ValidateDevCreds(devOptions))
                {
                    client = null;
                    result = new OperationResult()
                    {
                        IsSuccess = false,
                        EndUserMessage =
                            "The add on requires that developer credentials are specified but none were provided."
                    };
                    return result;
                }

                accessKey = devOptions.AccessKey;
                secretAccessKey = devOptions.SecretAccessKey;
            }

            client = new AmazonRedshiftClient(accessKey, secretAccessKey);
            result = new OperationResult { IsSuccess = true };
            return result;
        }

        private CreateClusterRequest CreateClusterRequest(RedshiftDeveloperOptions devOptions)
        {
            var request = new CreateClusterRequest()
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
                DBName = devOptions.DBName,
                ElasticIp = devOptions.ElasticIp,
                Encrypted = devOptions.Encrypted,
                HsmClientCertificateIdentifier = devOptions.HSMClientCertificateIdentifier,
                HsmConfigurationIdentifier = devOptions.HSMClientConfigurationIdentifier,
                MasterUsername = devOptions.MasterUserName,
                MasterUserPassword = devOptions.MasterPassword,
                NodeType = devOptions.NodeType,
                NumberOfNodes = devOptions.NumberOfNodes,
                Port = devOptions.Port,
                PreferredMaintenanceWindow = devOptions.PreferredMaintenanceWindow,
                PubliclyAccessible = devOptions.PubliclyAccessible,
                VpcSecurityGroupIds = devOptions.VpcSecurityGroupIds
            };
            return request;
        }

        private bool ValidateManifest(AddonManifest manifest, out OperationResult testResult)
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

            if (string.IsNullOrWhiteSpace(manifest.ProvisioningUsername) ||
                string.IsNullOrWhiteSpace(manifest.ProvisioningPassword))
            {
                testResult.IsSuccess = false;
                testResult.EndUserMessage = "Missing credentials 'provisioningUsername' & 'provisioningPassword' . These values needs to be provided as part of the manifest";
                return false;
            }

            return true;
        }
    }
}
