﻿namespace Apprenda.SaaSGrid.Addons.AWS.Redshift
{
    using Amazon.Redshift;
    using Amazon.Redshift.Model;
    using System;
    using System.Linq;
    using System.Threading;
    using Util;

    public class RedshiftAddOn : AddonBase
    {
        // Deprovision Redshift Instance
        // Input: AddonDeprovisionRequest request
        // Output: OperationResult
        public override OperationResult Deprovision(AddonDeprovisionRequest _request)
        {
            var connectionData = _request.ConnectionData;
            var deprovisionResult = new ProvisionAddOnResult(connectionData);

            try
            {
                var conInfo = RedshiftConnectionInfo.Parse(connectionData);
                var developerOptions = RedshiftDeveloperOptions.Parse(_request.DeveloperParameters, _request.Manifest);
                var client = EstablishClient(_request.Manifest);
                var response =
                    client.DeleteCluster(new DeleteClusterRequest
                    {
                        ClusterIdentifier = conInfo.ClusterIdentifier,
                        SkipFinalClusterSnapshot = developerOptions.SkipFinalSnapshot
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
                deprovisionResult.EndUserMessage =
                    "We tried to delete a cluster that no longer existed, resulting in no tranaction being committed.";
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
        public override ProvisionAddOnResult Provision(AddonProvisionRequest _request)
        {
            var provisionResult = new ProvisionAddOnResult("") { IsSuccess = false };
            try
            {
                var devOptions = RedshiftDeveloperOptions.Parse(_request.DeveloperParameters, _request.Manifest);
                var client = EstablishClient(_request.Manifest);
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
        public override OperationResult Test(AddonTestRequest _request)
        {
            var manifest = _request.Manifest;
            //var developerParameters = _request.DeveloperParameters;
            var testResult = new OperationResult { IsSuccess = false };
            if (manifest.Properties != null && manifest.Properties.Any())
            {
                //var devOptions = RedshiftDeveloperOptions.Parse(developerParameters, _request.Manifest);
                try
                {
                    //var client = 
                    EstablishClient(manifest);
                    //client.DescribeClusters();
                    testResult.IsSuccess = true;
                    testResult.EndUserMessage = "Tests completed successfully.";
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

        private static AmazonRedshiftClient EstablishClient(IAddOnDefinition _manifest)
        {
            var accessKey = _manifest.ProvisioningUsername;
            var secretAccessKey = _manifest.ProvisioningPassword;
            var awsRegionEndpoint = AwsUtils.ParseRegionEndpoint(_manifest.ProvisioningLocation);
            var config = new AmazonRedshiftConfig { RegionEndpoint = awsRegionEndpoint };
            return new AmazonRedshiftClient(accessKey, secretAccessKey, config);
        }

        private static CreateClusterRequest CreateClusterRequest(RedshiftDeveloperOptions _devOptions)
        {
            var request = new CreateClusterRequest
            {
                AllowVersionUpgrade = _devOptions.AllowVersionUpgrade,
                AutomatedSnapshotRetentionPeriod = (int)_devOptions.AutomatedSnapshotRetentionPeriod,
                //AvailabilityZone = _devOptions.AvailabilityZone,
                ClusterIdentifier = _devOptions.ClusterIdentifier,
                ClusterParameterGroupName = _devOptions.ClusterParameterGroupName,
                ClusterSecurityGroups = _devOptions.ClusterSecurityGroups,
                ClusterSubnetGroupName = _devOptions.ClusterSubnetGroupName,
                ClusterType = _devOptions.ClusterType,
                //ClusterVersion = _devOptions.ClusterVersion,
                DBName = _devOptions.DbName,
                //ElasticIp = _devOptions.ElasticIp,
                Encrypted = _devOptions.Encrypted,
                HsmClientCertificateIdentifier = _devOptions.HsmClientCertificateIdentifier,
                HsmConfigurationIdentifier = _devOptions.HsmClientConfigurationIdentifier,
                MasterUsername = _devOptions.MasterUserName,
                MasterUserPassword = _devOptions.MasterUserPassword,
                NodeType = _devOptions.NodeType,
                Port = (int)_devOptions.Port,
                PreferredMaintenanceWindow = _devOptions.PreferredMaintenanceWindow,
                PubliclyAccessible = _devOptions.PubliclyAccessible,
                VpcSecurityGroupIds = _devOptions.VpcSecurityGroupIds
            };
            if (request.ClusterType.Equals("multi-node"))
            {
                request.NumberOfNodes = (int)_devOptions.NumberOfNodes;
            }
            return request;
        }
    }
}