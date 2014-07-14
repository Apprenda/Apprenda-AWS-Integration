using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apprenda.SaaSGrid.Addons;
using Amazon.Glacier;
using Amazon.Glacier.Model;
using Amazon.Glacier.Transfer;
using System.Threading;

namespace Apprenda.SaaSGrid.Addons.AWS.Glacier
{
    public class GlacierAddon : AddonBase 
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
                AmazonGlacierClient client;
                var conInfo = ConnectionInfo.Parse(connectionData);
                var developerOptions = DeveloperOptions.Parse(devOptions);

                var establishClientResult = EstablishClient(manifest, developerOptions, out client);
                if (!establishClientResult.IsSuccess)
                {
                    deprovisionResult.EndUserMessage = establishClientResult.EndUserMessage;
                    return deprovisionResult;
                }
                var getArchivesInVault = client.DescribeVault(new DescribeVaultRequest() { AccountId = conInfo.AccountId, VaultName = conInfo.VaultName });
                var response =
                    client.DeleteVault(new DeleteVaultRequest()
                    {
                        AccountId = conInfo.AccountId,
                        VaultName = conInfo.VaultName
                    });
                // 5/22/14 fixing amazon aws deprecation
                if (response.HttpStatusCode != null)
                {
                    do
                    {
                        var verificationResponse = client.DescribeVault(new DescribeVaultRequest()
                        {
                            
                        });
                        // 5/22/14 fixing amazaon aws deprecration
                        if (verificationResponse == null)
                        {
                            deprovisionResult.IsSuccess = true;
                            break;
                        }
                        Thread.Sleep(TimeSpan.FromSeconds(10d));

                    } while (true);
                }
            }
            catch (ResourceNotFoundException)
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
                AmazonGlacierClient client;
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

                var response = client.CreateVault(CreateVaultRequest(devOptions));
                // fix 5/22/14 resolves amazon aws deprecation
                // wait for response to come back with a location
                while (true)
                {
                    if (response.Location != null)
                    {
                        var conInfo = new ConnectionInfo()
                        {
                            AccountId = devOptions.AccountId,
                            VaultName = devOptions.VaultName,
                            Location = response.Location
                        };
                        provisionResult.IsSuccess = true;
                        provisionResult.ConnectionData = conInfo.ToString();
                        break;
                    }
                    Thread.Sleep(10);
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
                    AmazonGlacierClient client;
                    var establishClientResult = EstablishClient(manifest, devOptions, out client);
                    if (!establishClientResult.IsSuccess)
                    {
                        return establishClientResult;
                    }
                    testProgress += establishClientResult.EndUserMessage;

                    var vaults = client.ListVaults();
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
            var result = new OperationResult() { IsSuccess = false };
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

        private OperationResult EstablishClient(AddonManifest manifest, DeveloperOptions devOptions, out AmazonGlacierClient client)
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

            client = new AmazonGlacierClient(accessKey, secretAccessKey);
            result = new OperationResult { IsSuccess = true };
            return result;
        }

        private CreateVaultRequest CreateVaultRequest(DeveloperOptions devOptions)
        {
            var request = new CreateVaultRequest()
            {
                // TODO - need to determine where defaults are used, and then not create the constructor where value is null (to use default)

                // These are required values.
                AccountId = devOptions.AccountId,
                VaultName = devOptions.VaultName
            };
            return request;
        }
    }
}
