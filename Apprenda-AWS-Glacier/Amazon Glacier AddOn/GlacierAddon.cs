using System;
using System.Linq;
using System.Threading;
using Amazon;
using Amazon.Glacier;
using Amazon.Glacier.Model;

namespace Apprenda.SaaSGrid.Addons.AWS.Glacier
{
    using Apprenda.SaaSGrid.Addons.AWS.Util;

    public class GlacierAddon : AddonBase
    {
        // Deprovision Glacier Instance
        // Input: AddonDeprovisionRequest request
        // Output: OperationResult
        public override OperationResult Deprovision(AddonDeprovisionRequest _request)
        {
            var connectionData = _request.ConnectionData;
            var deprovisionResult = new ProvisionAddOnResult(connectionData);
            var manifest = _request.Manifest;
            try
            {
                var conInfo = GlacierConnectionInfo.Parse(connectionData);
                var client = EstablishClient(manifest);
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
        public override ProvisionAddOnResult Provision(AddonProvisionRequest _request)
        {
            var provisionResult = new ProvisionAddOnResult("", false, "");
            try
            {
                var devOptions = GlacierDeveloperOptions.Parse(_request.DeveloperParameters, _request.Manifest);
                var client = EstablishClient(_request.Manifest);
                var response = client.CreateVault(CreateVaultRequest(devOptions, devOptions.AccountId));
                while (true)
                {
                    if (response.Location != null)
                    {
                        var conInfo = new GlacierConnectionInfo
                        {
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
        public override OperationResult Test(AddonTestRequest _request)
        {
            var testResult = new OperationResult {IsSuccess = false};
            if (_request.Manifest.Properties != null && _request.Manifest.Properties.Any())
            {
                if (!ValidateManifest(_request.Manifest))
                {
                    return testResult;
                }
                //var options = GlacierDeveloperOptions.Parse(_request.DeveloperParameters, _request.Manifest);
                try
                {
                    EstablishClient(_request.Manifest);
                    testResult.IsSuccess = true;
                    testResult.EndUserMessage = "Tests ran to success.";
                }
                catch (Exception e)
                {
                    testResult.EndUserMessage = e.Message;
                    Console.WriteLine(e.StackTrace);
                }
            }
            else
            {
                testResult.EndUserMessage = "Missing required manifest properties. Check your configuration.";
            }

            return testResult;
        }

        /* Begin private methods */

        private static bool ValidateManifest(IAddOnDefinition _manifest)
        {
            return !string.IsNullOrWhiteSpace(_manifest.ProvisioningUsername) &&
                   !string.IsNullOrWhiteSpace(_manifest.ProvisioningPassword);
        }

        private static AmazonGlacierClient EstablishClient(IAddOnDefinition _manifest)
        {
            var accessKey = _manifest.ProvisioningUsername;
            var secretAccessKey = _manifest.ProvisioningPassword;
            var location = AwsUtils.ParseRegionEndpoint(_manifest.ProvisioningLocation, true);
            var config = new AmazonGlacierConfig {RegionEndpoint = location};
            return new AmazonGlacierClient(accessKey, secretAccessKey, config);
        }

        private static CreateVaultRequest CreateVaultRequest(GlacierDeveloperOptions _devOptions, string _accountId)
        {
            return new CreateVaultRequest
            {
                AccountId = _accountId,
                VaultName = _devOptions.VaultName
            };
        }
    }
}