namespace Apprenda.SaaSGrid.Addons.AWS.RDS
{
    using Amazon.RDS;
    using Amazon.RDS.Model;
    using System;
    using System.Linq;
    using System.Threading;
    using Util;
    using Services.Logging;

    public class RdsAddOn : AddonBase
    {
        // logging variable
        private static readonly ILogger Log = LogManager.Instance().GetLogger(typeof(RdsAddOn));

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
                Log.Debug("AWS RDS AddOn: Established client connection");

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
                else
                {
                    Log.Debug(string.Format("AWS RDS AddOn: Failed to create database. Response Metadata {0}, HTTP status code {1}", response.ResponseMetadata.ToString(), response.HttpStatusCode));
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
                if (request.AvailabilityZone != null)
                {
                    request.AvailabilityZone = _devOptions.AvailabilityZone;
                }
            }
            if (_devOptions.OptionGroup != null)
            {
                request.OptionGroupName = _devOptions.OptionGroup;
            }
            if (_devOptions.Engine.Equals("oracle"))
            {
                request.Engine = _devOptions.OracleEngineEdition;
                request.EngineVersion = _devOptions.OracleDBVersion;
                request.Port = 1521; // default Oracle port
            }
            if (_devOptions.Engine.Equals("sqlserver"))
            {
                request.Engine = _devOptions.SqlServerEngineEdition;
                request.EngineVersion = _devOptions.SqlServerDBVersion;
                request.Port = 1433; // default SQL Server port
            }
            if (_devOptions.Engine.Equals("mysql"))
            {
                request.Engine = "MySQL";
                request.EngineVersion = _devOptions.MySqlDBVersion;
                request.Port = 3306; // default MySQL port
            }
            if (_devOptions.PreferredBackupWindow != null)
            {
                request.PreferredBackupWindow = _devOptions.PreferredBackupWindow;
            }
            if (_devOptions.PreferredMxWindow != null)
            {
                request.PreferredMaintenanceWindow = _devOptions.PreferredMxWindow;
            }


            // Debugging option only - printing all the options before we send them to AWS
            Log.Debug(string.Format("AllocatedStorage - {0}", request.AllocatedStorage));
            Log.Debug(string.Format("AutoMinorVersionUpgrade - {0}", request.AutoMinorVersionUpgrade));
            Log.Debug(string.Format("AvailabilityZone - {0}", request.AvailabilityZone));
            Log.Debug(string.Format("BackupRetentionPeriod - {0}", request.BackupRetentionPeriod));
            Log.Debug(string.Format("CharacterSetName - {0}", request.CharacterSetName));
            Log.Debug(string.Format("CopyTagsToSnapshot - {0}", request.CopyTagsToSnapshot));
            Log.Debug(string.Format("DBClusterIdentifier - {0}", request.DBClusterIdentifier));
            Log.Debug(string.Format("DBInstanceClass - {0}", request.DBInstanceClass));
            Log.Debug(string.Format("DBInstanceIdentifier - {0}", request.DBInstanceIdentifier));
            Log.Debug(string.Format("DBName - {0}", request.DBName));
            Log.Debug(string.Format("DBParameterGroupName - {0}", request.DBParameterGroupName));
            Log.Debug(string.Format("DBSecurityGroups - {0}", request.DBSecurityGroups));
            Log.Debug(string.Format("DBSubnetGroupName - {0}", request.DBSubnetGroupName));
            Log.Debug(string.Format("Engine - {0}", request.Engine));
            Log.Debug(string.Format("EngineVersion - {0}", request.EngineVersion));
            Log.Debug(string.Format("Iops - {0}", request.Iops));
            Log.Debug(string.Format("KmsKeyId - {0}", request.KmsKeyId));
            Log.Debug(string.Format("LicenseModel - {0}", request.LicenseModel));
            Log.Debug(string.Format("MasterUsername - {0}", request.MasterUsername));
            Log.Debug(string.Format("MasterUserPassword - {0}", request.MasterUserPassword));
            Log.Debug(string.Format("MonitoringInterval - {0}", request.MonitoringInterval));
            Log.Debug(string.Format("MonitoringRoleArn - {0}", request.MonitoringRoleArn));
            Log.Debug(string.Format("MultiAZ - {0}", request.MultiAZ));
            Log.Debug(string.Format("OptionGroupName - {0}", request.OptionGroupName));
            Log.Debug(string.Format("Port - {0}", request.Port));
            Log.Debug(string.Format("PreferredBackupWindow - {0}", request.PreferredBackupWindow));
            Log.Debug(string.Format("PreferredMaintenanceWindow - {0}", request.PreferredMaintenanceWindow));
            Log.Debug(string.Format("PubliclyAccessible - {0}", request.PubliclyAccessible));
            Log.Debug(string.Format("StorageEncrypted - {0}", request.StorageEncrypted));
            Log.Debug(string.Format("StorageType - {0}", request.StorageType));
            Log.Debug(string.Format("Tags - {0}", request.Tags));
            Log.Debug(string.Format("TdeCredentialArn - {0}", request.TdeCredentialArn));
            Log.Debug(string.Format("TdeCredentialPassword - {0}", request.TdeCredentialPassword));
            Log.Debug(string.Format("VpcSecurityGroupIds - {0}", request.VpcSecurityGroupIds));            

            return request;
        }
    }
}