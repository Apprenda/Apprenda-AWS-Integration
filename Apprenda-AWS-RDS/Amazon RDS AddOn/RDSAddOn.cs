namespace Apprenda.SaaSGrid.Addons.AWS.RDS
{
    using Amazon.RDS;
    using Amazon.RDS.Model;
    using System;
    using System.Linq;
    using System.Threading;
    using Util;

    public class RdsAddOn : AddonBase
    {
        // Deprovision RDS Instance
        // Input: AddonDeprovisionRequest request
        // Output: OperationResult
        public override OperationResult Deprovision(AddonDeprovisionRequest _request)
        {
            var connectionData = _request.ConnectionData;
            var deprovisionResult = new OperationResult();
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
                
                var response = client.CreateDBInstance(CreateDbInstanceRequest(devOptions));
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

        private static CreateDBInstanceRequest CreateDbInstanceRequest(RdsDeveloperOptions _devOptions)
        {
            var request = new CreateDBInstanceRequest
                              {
                                  // These are required values.
                                  BackupRetentionPeriod = _devOptions.BackupRetentionPeriod,
                                  AutoMinorVersionUpgrade = _devOptions.AutoMinorVersionUpgrade,
                                  AllocatedStorage = _devOptions.AllocatedStorage,
                                  DBInstanceClass = _devOptions.DbInstanceClass,
                                  DBInstanceIdentifier = _devOptions.DbInstanceIdentifier,
                                  MasterUsername = _devOptions.DbaUsername,
                                  MasterUserPassword = _devOptions.DbaPassword,
                                  Port = _devOptions.Port,
                                  PubliclyAccessible = _devOptions.PubliclyAccessible,
                              };
            // Oracle DB only parameter
            if (_devOptions.Engine.Equals("Oracle") && _devOptions.CharacterSet != null)
            {
                request.CharacterSetName = _devOptions.CharacterSet;
            }
            if (_devOptions.DbSecurityGroups != null)
            {
                request.DBSecurityGroups = _devOptions.DbSecurityGroups;
            }
            if (_devOptions.SubnetGroupName != null)
            {
                request.DBSubnetGroupName = _devOptions.SubnetGroupName;
            }
            if (_devOptions.LicenseModel != null)
            {
                request.LicenseModel = _devOptions.LicenseModel;
            }
            if (!_devOptions.Engine.Equals("sqlserver"))
            {
                request.DBName = _devOptions.DbName;
            }
            if (_devOptions.MultiAz)
            {
                request.MultiAZ = _devOptions.MultiAz;
            }
            else
            {
                if(request.AvailabilityZone != null) request.AvailabilityZone = _devOptions.AvailabilityZone;
            }
            if (_devOptions.OptionGroup != null)
            {
                request.OptionGroupName = _devOptions.OptionGroup;
            }
            if (_devOptions.Engine.Equals("oracle"))
            {
                request.Engine = _devOptions.OracleEngineEdition;
                request.EngineVersion = _devOptions.OracleDBVersion;
            }
            if (_devOptions.Engine.Equals("sqlserver"))
            {
                request.Engine = _devOptions.SqlServerEngineEdition;
                request.EngineVersion = _devOptions.SqlServerDBVersion;
            }
            if (_devOptions.Engine.Equals("mysql"))
            {
                request.Engine = "MySQL";
                request.EngineVersion = _devOptions.MySqlDBVersion;
            }
            if (_devOptions.PreferredBackupWindow != null)
            {
                request.PreferredBackupWindow = _devOptions.PreferredBackupWindow;
            }
            if (_devOptions.PreferredMxWindow != null)
            {
                request.PreferredMaintenanceWindow = _devOptions.PreferredMxWindow;
            }
            return request;
        }
    }
}