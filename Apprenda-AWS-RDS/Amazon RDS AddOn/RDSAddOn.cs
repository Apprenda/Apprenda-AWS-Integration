using Amazon;
using Amazon.RDS;
using Amazon.RDS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Apprenda.SaaSGrid.Addons.AWS.RDS
{
    public class RdsAddOn : AddonBase
    {
        // Deprovision RDS Instance
        // Input: AddonDeprovisionRequest request
        // Output: OperationResult
        public override OperationResult Deprovision(AddonDeprovisionRequest request)
        {
            var connectionData = request.ConnectionData;
            var deprovisionResult = new ProvisionAddOnResult(connectionData);
            var manifest = request.Manifest;
            var devOptions = request.DeveloperParameters;
            try
            {
                AmazonRDSClient client;
                var conInfo = RDSConnectionInfo.Parse(connectionData);
                var developerOptions = RDSDeveloperOptions.Parse(devOptions);

                var establishClientResult = EstablishClient(manifest, developerOptions, out client);
                if (!establishClientResult.IsSuccess)
                {
                    deprovisionResult.EndUserMessage = establishClientResult.EndUserMessage;
                    return deprovisionResult;
                }

                var response = client.DeleteDBInstance(new DeleteDBInstanceRequest
                        {
                            DBInstanceIdentifier = conInfo.DbInstanceIdentifier,
                            SkipFinalSnapshot = true
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
        public override ProvisionAddOnResult Provision(AddonProvisionRequest request)
        {
            var provisionResult = new ProvisionAddOnResult("");
            var manifest = request.Manifest;
            var developerOptions = request.DeveloperParameters;

            try
            {
                AmazonRDSClient client;
                RDSDeveloperOptions devOptions;

                var parseOptionsResult = ParseDevOptions(developerOptions, out devOptions);
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
        public override OperationResult Test(AddonTestRequest request)
        {
            var manifest = request.Manifest;
            var developerOptions = request.DeveloperParameters;
            var testResult = new OperationResult { IsSuccess = false };
            var testProgress = "";

            if (manifest.Properties != null && manifest.Properties.Any())
            {
                RDSDeveloperOptions devOptions;

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
                    AmazonRDSClient client;
                    var establishClientResult = EstablishClient(manifest, devOptions, out client);
                    if (!establishClientResult.IsSuccess)
                    {
                        return establishClientResult;
                    }
                    testProgress += establishClientResult.EndUserMessage;

                    client.DescribeDBInstances();
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

        private bool ValidateDevCreds(RDSDeveloperOptions devOptions)
        {
            return !(string.IsNullOrWhiteSpace(devOptions.AccessKey) || string.IsNullOrWhiteSpace(devOptions.SecretAccessKey));
        }

        private OperationResult ParseDevOptions(IEnumerable<AddonParameter> developerOptions, out RDSDeveloperOptions devOptions)
        {
            var result = new OperationResult { IsSuccess = false };
            var progress = "";
            try
            {
                progress += "Parsing developer options...\n";
                devOptions = RDSDeveloperOptions.Parse(developerOptions);
            }
            catch (ArgumentException e)
            {
                result.EndUserMessage = e.Message;
                devOptions = null;
                return result;
            }

            result.IsSuccess = true;
            result.EndUserMessage = progress;
            return result;
        }

        private OperationResult EstablishClient(AddonManifest manifest, RDSDeveloperOptions devOptions, out AmazonRDSClient client)
        {
            OperationResult result;

            bool requireCreds;
            var manifestProps = manifest.GetProperties().ToDictionary(x => x.Key, x => x.Value);
            var accessKey = manifestProps["AWSClientKey"];
            var secretAccessKey = manifestProps["AWSSecretKey"];

            // we *should* be using this...
            //var regionEndpoint = manifestProps["AWSRegionEndpoint"];

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

                accessKey = devOptions.AccessKey;
                secretAccessKey = devOptions.SecretAccessKey;
            }
            var config = new AmazonRDSConfig { RegionEndpoint = RegionEndpoint.USEast1 };
            client = new AmazonRDSClient(accessKey, secretAccessKey, config);
            result = new OperationResult { IsSuccess = true };
            return result;
        }

        private CreateDBInstanceRequest CreateDbInstanceRequest(RDSDeveloperOptions devOptions)
        {
            var request = new CreateDBInstanceRequest
            {
                // These are required values.
                BackupRetentionPeriod = devOptions.BackupRetentionPeriod,
                DBParameterGroupName = devOptions.DbParameterGroupName,
                DBSecurityGroups = devOptions.DBSecurityGroups,
                DBSubnetGroupName = devOptions.SubnetGroupName,
                DBInstanceClass = devOptions.DbInstanceClass,
                DBInstanceIdentifier = devOptions.DbInstanceIdentifier,
                DBName = devOptions.DbName,
                Engine = devOptions.Engine,
                EngineVersion = devOptions.EngineVersion,
                LicenseModel = devOptions.LicenseModel,
                MasterUsername = devOptions.DBAUsername,
                MasterUserPassword = devOptions.DBAPassword,
                Iops = devOptions.ProvisionedIOPs,
                MultiAZ = devOptions.MultiAz,
                OptionGroupName = devOptions.OptionGroup,
                Port = devOptions.Port,
                PreferredBackupWindow = devOptions.PreferredBackupWindow,
                PreferredMaintenanceWindow = devOptions.PreferredMXWindow,
                PubliclyAccessible = devOptions.PubliclyAccessible,
                Tags = devOptions.Tags,
                VpcSecurityGroupIds = devOptions.VpcSecurityGroupIds
            };

            if (!devOptions.MultiAz)
            {
                request.AvailabilityZone = devOptions.AvailabilityZone;
            }

            // Oracle DB only parameter
            if (request.Engine.Equals("Oracle") && devOptions.CharacterSet.Length > 0)
            {
                request.CharacterSetName = devOptions.CharacterSet;
            }
            // default is 0, if specified change it
            if (devOptions.AllocatedStorage > 0)
            {
                request.AllocatedStorage = devOptions.AllocatedStorage;
            }
            // default is false, if true change it
            if (devOptions.AutoMinorVersionUpgrade)
            {
                request.AutoMinorVersionUpgrade = devOptions.AutoMinorVersionUpgrade;
            }
            return request;
        }
    }
}