namespace Apprenda.SaaSGrid.Addons.AWS.RDS
{
    using Amazon;
    using Amazon.RDS;
    using Amazon.RDS.Model;
    using System;
    using System.Linq;
    using System.Threading;

    using Amazon.Util;

    using Apprenda.SaaSGrid.Addons.AWS.Util;

    public class RdsAddOn : AddonBase
    {
        // Deprovision RDS Instance
        // Input: AddonDeprovisionRequest request
        // Output: OperationResult
        public override OperationResult Deprovision(AddonDeprovisionRequest _request)
        {
            var connectionData = _request.ConnectionData;
            var deprovisionResult = new ProvisionAddOnResult(connectionData);
            var manifest = _request.Manifest;
            var devOptions = _request.DeveloperParameters;
            try
            {
                var conInfo = RDSConnectionInfo.Parse(connectionData);
                var developerOptions = RdsDeveloperOptions.Parse(devOptions, manifest);
                var client = EstablishClient(manifest);
                var response = client.DeleteDBInstance(new DeleteDBInstanceRequest
                        {
                            DBInstanceIdentifier = conInfo.DbInstanceIdentifier,
                            SkipFinalSnapshot = developerOptions.SkipFinalSnapshot
                        });
                if (response.DBInstance != null)
                {
                    do
                    {
                        var verificationResponse = client.DescribeDBInstances(new DescribeDBInstancesRequest
                            {
                                DBInstanceIdentifier = conInfo.DbInstanceIdentifier
                            });
                        if (!verificationResponse.DBInstances.Any())
                        {
                            deprovisionResult.IsSuccess = true;
                            break;
                        }
                        Thread.Sleep(TimeSpan.FromSeconds(10d));
                    } while (true);
                }
            }
            catch (DBInstanceNotFoundException)
            {
                deprovisionResult.IsSuccess = true;
            }
            catch (Exception e)
            {
                deprovisionResult.EndUserMessage = e.Message;
            }

            return deprovisionResult;
        }

        // Provision RDS Instance
        // Input: AddonDeprovisionRequest request
        // Output: ProvisionAddOnResult
        public override ProvisionAddOnResult Provision(AddonProvisionRequest _request)
        {
            var provisionResult = new ProvisionAddOnResult("");
            
            try
            {
                var devOptions = RdsDeveloperOptions.Parse(_request.DeveloperParameters, _request.Manifest);
                var client = EstablishClient(_request.Manifest);
                
                var response = client.CreateDBInstance(this.CreateDbInstanceRequest(devOptions));
                if (response.DBInstance != null)
                {
                    do
                    {
                        var verificationResponse = client.DescribeDBInstances(new DescribeDBInstancesRequest
                            {
                                DBInstanceIdentifier = devOptions.DbInstanceIdentifier
                            });
                        if (verificationResponse.DBInstances.Any() && verificationResponse.DBInstances[0].DBInstanceStatus == "available")
                        {
                            var dbInstance = verificationResponse.DBInstances[0];
                            var conInfo = new RDSConnectionInfo
                                {
                                    DbInstanceIdentifier = devOptions.DbInstanceIdentifier,
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
            var testResult = new OperationResult { IsSuccess = false };

            if (manifest.Properties != null && manifest.Properties.Any())
            {
                if (!ValidateManifest(manifest))
                {
                    testResult.EndUserMessage = "Manifest failed validation. Check your settings.";
                    return testResult;
                }

                //var devOptions = RdsDeveloperOptions.Parse(_request.DeveloperParameters, _request.Manifest);

                try
                {
                    var client = EstablishClient(manifest);
                    client.DescribeDBInstances();
                    testResult.IsSuccess = true;
                    testResult.EndUserMessage = "Tests completed.";
                }
                catch (Exception e)
                {
                    testResult.EndUserMessage = e.Message;
                }
            }
            else
            {
                testResult.EndUserMessage = "Missing required manifest properties. Please check to make sure your settings are valid.";
            }
            return testResult;
        }

        private static bool ValidateManifest(IAddOnDefinition _manifest)
        {
            return !string.IsNullOrWhiteSpace(_manifest.ProvisioningUsername)
                   && !string.IsNullOrWhiteSpace(_manifest.ProvisioningPassword);
        }

        private static AmazonRDSClient EstablishClient(IAddOnDefinition _manifest)
        {
            var accessKey = _manifest.ProvisioningUsername;
            var secretAccessKey = _manifest.ProvisioningPassword;
            var location = AwsUtils.ParseRegionEndpoint(_manifest.ProvisioningLocation, true);
            var config = new AmazonRDSConfig { RegionEndpoint = location };
            return new AmazonRDSClient(accessKey, secretAccessKey, config);
        }

        private CreateDBInstanceRequest CreateDbInstanceRequest(RdsDeveloperOptions _devOptions)
        {
            var request = new CreateDBInstanceRequest
                              {
                                  // These are required values.
                                  BackupRetentionPeriod = _devOptions.BackupRetentionPeriod,
                                  //DBParameterGroupName = devOptions.DbParameterGroupName,
                                  DBSecurityGroups = _devOptions.DbSecurityGroups,
                                  DBSubnetGroupName = _devOptions.SubnetGroupName,
                                  DBInstanceClass = _devOptions.DbInstanceClass,
                                  DBInstanceIdentifier = _devOptions.DbInstanceIdentifier,
                                  DBName = _devOptions.DbName,
                                  Engine = _devOptions.Engine,
                                  EngineVersion = _devOptions.EngineVersion,
                                  LicenseModel = _devOptions.LicenseModel,
                                  MasterUsername = _devOptions.DbaUsername,
                                  MasterUserPassword = _devOptions.DbaPassword,
                                  Iops = _devOptions.ProvisionedIoPs,
                                  //MultiAZ = devOptions.MultiAz,
                                  OptionGroupName = _devOptions.OptionGroup,
                                  Port = _devOptions.Port,
                                  PreferredBackupWindow = _devOptions.PreferredBackupWindow,
                                  PreferredMaintenanceWindow = _devOptions.PreferredMxWindow,
                                  PubliclyAccessible = _devOptions.PubliclyAccessible,
                                  AvailabilityZone = _devOptions.AvailabilityZone,
                                  //Tags = devOptions.Tags,
                                  //VpcSecurityGroupIds = devOptions.VpcSecurityGroupIds
                              };

            // Oracle DB only parameter
            if (request.Engine.Equals("Oracle") && _devOptions.CharacterSet.Length > 0)
            {
                request.CharacterSetName = _devOptions.CharacterSet;
            }
            // default is 0, if specified change it
            if (_devOptions.AllocatedStorage > 0)
            {
                request.AllocatedStorage = _devOptions.AllocatedStorage;
            }
            // default is false, if true change it
            if (_devOptions.AutoMinorVersionUpgrade)
            {
                request.AutoMinorVersionUpgrade = _devOptions.AutoMinorVersionUpgrade;
            }
            return request;
        }
    }
}