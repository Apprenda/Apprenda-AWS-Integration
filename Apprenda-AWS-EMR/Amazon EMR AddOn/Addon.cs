using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apprenda.SaaSGrid.Addons;

namespace Amazon_Base_Addon
{
    public abstract class Addon : AddonBase
    {
        // The first three methods here will need to be overridden. I've extended to add some utility classes that are non-component specific.

        // Deprovision Action
        // Input: AddonDeprovisionRequest
        // Output: OpertaionResult
        public abstract override OperationResult Deprovision(AddonDeprovisionRequest request);

        // Provision Instance
        // Input: AddonDeprovisionRequest request
        // Output: ProvisionAddOnResult
        public abstract override ProvisionAddOnResult Provision(AddonProvisionRequest request);

        // Testing Instance
        // Input: AddonTestRequest request
        // Output: OperationResult
        public abstract override OperationResult Test(AddonTestRequest request);

        public bool ValidateManifest(AddonManifest manifest, out OperationResult testResult)
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
