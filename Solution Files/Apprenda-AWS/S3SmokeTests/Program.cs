using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apprenda.SaaSGrid.Addons;
using Apprenda.SaaSGrid.Addons.AWS.S3;
namespace S3SmokeTests
{
    class Program
    {
        static void Main(string[] args)
        {
            S3Addon addon = new S3Addon();
            var _developerOptions = "BucketName=com.dutra.apprenda";
            var _manifest = new AddonManifest()
            {
                ProvisioningUsername = "chris@dutronlabs.com",
                ProvisioningPassword = "cyrixm2r",
                Properties = {
                                new AddonProperty() { Key="RequireDevCredentials", Value="false"},
                                new AddonProperty() { Key="AWSClientKey", Value="AKIAIATHFDT32A7C3GFA"},
                                new AddonProperty() { Key="AWSSecretKey", Value="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"},
                                new AddonProperty() { Key="RegionEndpoint", Value="USEast1"}
                            }
            };
            var result = addon.Provision(new AddonProvisionRequest()
                {
                    DeveloperOptions= _developerOptions,
                    Manifest = _manifest
                });

            var d_result = addon.Deprovision(new AddonDeprovisionRequest()
                {
                    DeveloperOptions = _developerOptions,
                    Manifest = _manifest,
                    ConnectionData = result.ConnectionData
                });
            Console.WriteLine(result.IsSuccess);
            Console.WriteLine(result.EndUserMessage);
            Console.WriteLine(result.ConnectionData);
            Console.WriteLine(d_result.IsSuccess);
            Console.WriteLine(d_result.EndUserMessage);
        }
    }
}
