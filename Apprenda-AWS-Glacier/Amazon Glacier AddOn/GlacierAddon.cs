using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Amazon;
using Amazon.Glacier;
using Amazon.Glacier.Model;

namespace Apprenda.SaaSGrid.Addons.AWS.Glacier
{
    public class GlacierAddon : AddonBase
    {
        // Deprovision Glacier Instance
        // Input: AddonDeprovisionRequest request
        // Output: OperationResult
        public override OperationResult Deprovision(AddonDeprovisionRequest request)
        {
            var connectionData = request.ConnectionData;
            var deprovisionResult = new ProvisionAddOnResult(connectionData);
            var manifest = request.Manifest;
            try
            {
                AmazonGlacierClient client;
                var conInfo = GlacierConnectionInfo.Parse(connectionData);
                var establishClientResult = EstablishClient(manifest, out client);
                if (!establishClientResult.IsSuccess)
                {
                    deprovisionResult.EndUserMessage = establishClientResult.EndUserMessage;
                    return deprovisionResult;
                }
                client.DeleteVault(new DeleteVaultRequest
                {
                    AccountId = conInfo.AccountId,
                    VaultName = conInfo.VaultName
                });
                do
                {
                    var verificationResponse = client.DescribeVault(new DescribeVaultRequest());
                    if (verificationResponse == null)
                    {
                        deprovisionResult.IsSuccess = true;
                        break;
                    }
                    Thread.Sleep(TimeSpan.FromSeconds(10d));
                } while (true);
                
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
            var provisionResult = new ProvisionAddOnResult("");
            var manifest = request.Manifest;
            var developerParameters = request.DeveloperParameters;

            try
            {
                AmazonGlacierClient client;
                GlacierDeveloperOptions devOptions;

                // ReSharper disable MaximumChainedReferences
                var accountId = manifest.Properties.Find(property => property.Key.Equals("AWSAccountID")).Value;

                var parseOptionsResult = ParseDevOptions(developerParameters, out devOptions);
                if (!parseOptionsResult.IsSuccess)
                {
                    provisionResult.EndUserMessage = parseOptionsResult.EndUserMessage;
                    return provisionResult;
                }

                var establishClientResult = EstablishClient(manifest, out client);
                if (!establishClientResult.IsSuccess)
                {
                    provisionResult.EndUserMessage = establishClientResult.EndUserMessage;
                    return provisionResult;
                }

                var response = client.CreateVault(CreateVaultRequest(devOptions, accountId));
                while (true)
                {
                    if (response.Location != null)
                    {
                        var conInfo = new GlacierConnectionInfo
                        {
                            AccountId = accountId,
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
            var developerParameters = request.DeveloperParameters;
            var testResult = new OperationResult {IsSuccess = false};
            string testProgress = "";

            if (manifest.Properties != null && manifest.Properties.Any())
            {
                GlacierDeveloperOptions devOptions;

                testProgress += "Evaluating required manifest properties...\n";
                if (!ValidateManifest(manifest, out testResult))
                {
                    return testResult;
                }

                OperationResult parseOptionsResult = ParseDevOptions(developerParameters, out devOptions);
                if (!parseOptionsResult.IsSuccess)
                {
                    return parseOptionsResult;
                }
                testProgress += parseOptionsResult.EndUserMessage;

                try
                {
                    testProgress += "Establishing connection to AWS...\n";
                    AmazonGlacierClient client;
                    OperationResult establishClientResult = EstablishClient(manifest, out client);
                    if (!establishClientResult.IsSuccess)
                    {
                        return establishClientResult;
                    }
                    testProgress += establishClientResult.EndUserMessage;
                    testProgress += "Successfully passed all testing criteria!";
                    testResult.IsSuccess = true;
                    testResult.EndUserMessage = testProgress;
                }
                catch (Exception e)
                {
                    testResult.EndUserMessage = e.Message;
                    Console.WriteLine(e.StackTrace);
                }
            }
            else
            {
                testResult.EndUserMessage = "Missing required manifest properties (requireDevCredentials)";
            }

            return testResult;
        }

        /* Begin private methods */

        private static bool ValidateManifest(AddonManifest manifest, out OperationResult testResult)
        {
            testResult = new OperationResult();

            var prop =
                manifest.Properties.FirstOrDefault(
                    p => p.Key.Equals("requireDevCredentials", StringComparison.InvariantCultureIgnoreCase));

            if (prop == null || !prop.HasValue)
            {
                testResult.IsSuccess = false;
                testResult.EndUserMessage =
                    "Missing required property 'requireDevCredentials'. This property needs to be provided as part of the manifest";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(manifest.ProvisioningUsername) &&
                !string.IsNullOrWhiteSpace(manifest.ProvisioningPassword)) return true;
            testResult.IsSuccess = false;
            testResult.EndUserMessage =
                "Missing credentials 'provisioningUsername' & 'provisioningPassword' . These values needs to be provided as part of the manifest";
            return false;
        }

        private static OperationResult ParseDevOptions(IEnumerable<AddonParameter> developerParameters,
            out GlacierDeveloperOptions devOptions)
        {
            devOptions = null;
            var result = new OperationResult {IsSuccess = false};

            try
            {
                devOptions = GlacierDeveloperOptions.Parse(developerParameters);
            }
            catch (ArgumentException e)
            {
                result.EndUserMessage = e.Message;
                return result;
            }

            result.IsSuccess = true;
            return result;
        }

        private static OperationResult EstablishClient(IAddOnDefinition manifest,
            out AmazonGlacierClient client)
        {
            var accessKey = manifest.ProvisioningUsername;
            var secretAccessKey = manifest.ProvisioningPassword;

            var config = new AmazonGlacierConfig {RegionEndpoint = RegionEndpoint.USEast1};
            client = new AmazonGlacierClient(accessKey, secretAccessKey, config);
            var result = new OperationResult {IsSuccess = true};
            return result;
        }

        private static CreateVaultRequest CreateVaultRequest(GlacierDeveloperOptions devOptions, string accountId)
        {
            var request = new CreateVaultRequest
            {
                AccountId = accountId,
                VaultName = devOptions.VaultName
            };
            return request;
        }
    }
}