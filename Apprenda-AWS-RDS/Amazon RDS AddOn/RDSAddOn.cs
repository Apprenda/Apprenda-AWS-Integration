using Amazon.RDS.Model;
using Apprenda.SaaSGrid.Addons;
using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.RDS;
using System.Threading;

namespace Apprenda.SaaSGrid.Addons.AWS.RDS
{
    public class RDSAddOn : AddonBase
    {
        // Deprovision RDS Instance
        // Input: AddonDeprovisionRequest request
        // Output: OperationResult
        public override OperationResult Deprovision(AddonDeprovisionRequest request)
        {
            string connectionData = request.ConnectionData;
            // changing to overloaded constructor - 5/22/14
            var deprovisionResult = new ProvisionAddOnResult(connectionData);
            AddonManifest manifest = request.Manifest;
            string devOptions = request.DeveloperOptions;
            
            try
            {
                AmazonRDSClient client;
                var conInfo = ConnectionInfo.Parse(connectionData);
                var developerOptions = DeveloperOptions.Parse(devOptions);

                var establishClientResult = EstablishClient(manifest, developerOptions, out client);
                if (!establishClientResult.IsSuccess)
                {
                    deprovisionResult.EndUserMessage = establishClientResult.EndUserMessage;
                    return deprovisionResult;
                }

                var response =
                    client.DeleteDBInstance(new DeleteDBInstanceRequest()
                        {
                            DBInstanceIdentifier = conInfo.DbInstanceIdentifier,
                            SkipFinalSnapshot = true
                        });
                // 5/22/14 fixing amazon aws deprecation
                if (response.DBInstance != null)
                {
                    do
                    {
                        var verificationResponse = client.DescribeDBInstances(new DescribeDBInstancesRequest()
                            {
                                DBInstanceIdentifier = conInfo.DbInstanceIdentifier
                            });
                        // 5/22/14 fixing amazaon aws deprecration
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
            // i think this is a bug. but I'm going to throw an empty string to it to clear the warning.
            var provisionResult = new ProvisionAddOnResult("");
            AddonManifest manifest = request.Manifest;
            string developerOptions = request.DeveloperOptions;

            try
            {
                AmazonRDSClient client;
                DeveloperOptions devOptions;

                var parseOptionsResult = ParseDevOptions(developerOptions, out devOptions);
                if (!parseOptionsResult.IsSuccess)
                {
                    provisionResult.EndUserMessage = parseOptionsResult.EndUserMessage;
                    return provisionResult;
                }

                var establishClientResult = EstablishClient(manifest, DeveloperOptions.Parse(developerOptions), out client);
                if (!establishClientResult.IsSuccess)
                {
                    provisionResult.EndUserMessage = establishClientResult.EndUserMessage;
                    return provisionResult;
                }

                var response = client.CreateDBInstance(CreateDbInstanceRequest(devOptions));
                // fix 5/22/14 resolves amazon aws deprecation
                if (response.DBInstance != null)
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
                        var verificationResponse = client.DescribeDBInstances(new DescribeDBInstancesRequest()
                            {
                                DBInstanceIdentifier = devOptions.DbInstanceIdentifier
                            });
                        // fix on next few lines 5/22/14 resolve amazon aws deprecation.
                        if (verificationResponse.DBInstances.Any() && verificationResponse.DBInstances[0].DBInstanceStatus == "available")
                        {
                            var dbInstance = verificationResponse.DBInstances[0];
                            var conInfo = new ConnectionInfo()
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
            AddonManifest manifest = request.Manifest;
            string developerOptions = request.DeveloperOptions;
            var testResult = new OperationResult {IsSuccess = false};
            var testProgress = "";

            if (manifest.Properties != null && manifest.Properties.Any())
            {
                DeveloperOptions devOptions;

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

        /* Begin private methods */

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

        // TODO: We might be able to extend this. 
        private bool ValidateDevCreds(DeveloperOptions devOptions)
        {
            return !(string.IsNullOrWhiteSpace(devOptions.AccessKey) || string.IsNullOrWhiteSpace(devOptions.SecretAccessKey));
        }

        private OperationResult ParseDevOptions(string developerOptions, out DeveloperOptions devOptions)
        {
            devOptions = null;
            var result = new OperationResult(){IsSuccess = false};
            var progress = "";

            try
            {
                progress += "Parsing developer options...\n";
                devOptions = DeveloperOptions.Parse(developerOptions);
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

        private OperationResult EstablishClient(AddonManifest manifest, DeveloperOptions devOptions, out AmazonRDSClient client)
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
            
            client = new AmazonRDSClient(accessKey, secretAccessKey);
            result = new OperationResult { IsSuccess = true };
            return result;
        }

        private CreateDBInstanceRequest CreateDbInstanceRequest(DeveloperOptions devOptions)
        {
            var request = new CreateDBInstanceRequest()
            {
                // TODO - need to determine where defaults are used, and then not create the constructor where value is null (to use default)
                
                // These are required values.
                BackupRetentionPeriod = devOptions.BackupRetentionPeriod,
                DBParameterGroupName = devOptions.DBParameterGroupName,
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
                MultiAZ = devOptions.MultiAZ,
                OptionGroupName = devOptions.OptionGroup,
                Port = devOptions.Port,
                PreferredBackupWindow = devOptions.PreferredBackupWindow,
                PreferredMaintenanceWindow = devOptions.PreferredMXWindow,
                PubliclyAccessible = devOptions.PubliclyAccessible,
                Tags = devOptions.Tags,
                VpcSecurityGroupIds = devOptions.VPCSecurityGroupIds
            };

            if(!devOptions.MultiAZ)
            {
                request.AvailabilityZone = devOptions.AvailabilityZone;
            }

            // Oracle DB only parameter
            if(request.Engine.Equals("Oracle") && devOptions.CharacterSet != null)
            {
                request.CharacterSetName = devOptions.CharacterSet;
            }

            if (devOptions.AllocatedStorage != null)
            {
                request.AllocatedStorage = devOptions.AllocatedStorage;
            }

            if (devOptions.AutoMinorVersionUpgrade != null)
            {
                request.AutoMinorVersionUpgrade = devOptions.AutoMinorVersionUpgrade;
            }
            return request;
        }
    }
}
