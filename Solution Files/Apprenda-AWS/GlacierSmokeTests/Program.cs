using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apprenda.SaaSGrid.Addons.AWS.Glacier;
using Apprenda.SaaSGrid.Addons;

namespace GlacierSmokeTests
{
    class Program
    {
        static void Main(string[] args)
        {
            GlacierAddon addon = new GlacierAddon();
            var result = addon.Test(new AddonTestRequest()
                {
                    DeveloperOptions ="VaultName=test",
                    Manifest = new AddonManifest()
                    {
                        ProvisioningUsername="chris@dutronlabs.com",
                        ProvisioningPassword="cyrixm2r",
                        Properties = { new AddonProperty(){ Key="requireDevCredentials", Value="false"}, 
                            new AddonProperty() { Key="AWSClientKey", Value="AKIAIATHFDT32A7C3GFA" }, 
                            new AddonProperty() { Key="AWSSecretKey", Value="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"},
                            new AddonProperty() { Key="AWSAccountID", Value="932190250616"}
                        }
                    }
                });
            Console.WriteLine(result.EndUserMessage);
        }
    }
}
