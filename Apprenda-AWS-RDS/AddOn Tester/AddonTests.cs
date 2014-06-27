using Amazon_RDS_AddOn;
using Apprenda.SaaSGrid.Addons;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AddOn_Tester
{
    [TestFixture]
    public class AddonTests
    {
        private const string awsAccessKey = "replace-with-aws-access-key";
        private const string awsSecretKey = "replace-with-aws-secret-key";

        [Test]
        public static void TestManifestMissingProperties()
        {
            var addon = new Addon();
            var manifest = GetAWSAddonManifest();
            manifest.Properties.Clear();
            var request = new AddonTestRequest {Manifest = manifest, DeveloperOptions = ""};

            var result = addon.Test(request);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.EndUserMessage.Contains("Missing required manifest properties"), result.EndUserMessage);

            manifest = GetAWSAddonManifest();
            manifest.ProvisioningUsername = null;
            manifest.ProvisioningPassword = "pass";
            request.Manifest = manifest;
            result = addon.Test(request);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.EndUserMessage.Contains("needs to be provided as part of the manifest"), result.EndUserMessage);

            manifest = GetAWSAddonManifest();
            manifest.ProvisioningUsername = "user";
            manifest.ProvisioningPassword = null;
            request.Manifest = manifest;
            result = addon.Test(request);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.EndUserMessage.Contains("needs to be provided as part of the manifest"), result.EndUserMessage);
        }

        [Test]
        public static void TestManifestMissingCredProperty()
        {
            var addon = new Addon();
            var manifest = GetAWSAddonManifest();
            manifest.Properties.Clear();
            manifest.Properties.Add(new AddonProperty{Key = "randomProperty", Value = "randomValue"});
            var request = new AddonTestRequest { Manifest = manifest, DeveloperOptions = "" };
            var result = addon.Test(request);

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.EndUserMessage.Contains("Missing required property"), result.EndUserMessage);
        }

        [Test]
        public static void TestManifestMissingDefaultCreds()
        {
            var addon = new Addon();
            var manifest = GetAWSAddonManifest();
            var result = new OperationResult();
            var request = new AddonTestRequest { Manifest = manifest, DeveloperOptions = "" };

            manifest.ProvisioningUsername = null;
            manifest.ProvisioningPassword = "Default";
            result = addon.Test(request);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.EndUserMessage.Contains("Missing credentials"), result.EndUserMessage);

            manifest.ProvisioningUsername = "Default";
            manifest.ProvisioningPassword = null;
            result = addon.Test(request);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.EndUserMessage.Contains("Missing credentials"), result.EndUserMessage);

            manifest.ProvisioningUsername = null;
            manifest.ProvisioningPassword = null;
            result = addon.Test(request);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.EndUserMessage.Contains("Missing credentials"), result.EndUserMessage);
        }

        [Test]
        public static void TestMissingDevCreds()
        {
            var addon = new Addon();
            var manifest = GetAWSAddonManifest();
            var request = new AddonTestRequest { Manifest = manifest, DeveloperOptions = "" };

            var result = addon.Test(request);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.EndUserMessage.Contains("requires that developer credentials are specified"), result.EndUserMessage);

            request.DeveloperOptions = "accessKey=abe";
            result = addon.Test(request);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.EndUserMessage.Contains("requires that developer credentials are specified"), result.EndUserMessage);

            request.DeveloperOptions = "secretKey=secret";
            result = addon.Test(request);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.EndUserMessage.Contains("requires that developer credentials are specified"), result.EndUserMessage);
        }

        [Test]
        public static void TestInvalidDevOptions()
        {
            var addon = new Addon();
            var manifest = GetAWSAddonManifest();
            var request = new AddonTestRequest { Manifest = manifest };
            
            request.DeveloperOptions = "=&=";
            var result = addon.Test(request);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.EndUserMessage.Contains("Unable to parse developer options"), result.EndUserMessage);

            request.DeveloperOptions = "random=key&";
            result = addon.Test(request);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.EndUserMessage.Contains("was not expected and is not understood"), result.EndUserMessage);

            request.DeveloperOptions = "allocatedstorage=five";
            result = addon.Test(request);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.EndUserMessage.Contains("can only have an integer value"), result.EndUserMessage);

            request.DeveloperOptions = "autominorversionupgrade=1";
            result = addon.Test(request);
            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.EndUserMessage.Contains("only have a value of true|false"), result.EndUserMessage);
        }

        [Test]
        public static void TestSuccessfullyConnectionToAWS()
        {
            var addon = new Addon();
            var manifest = GetAWSAddonManifest();
            manifest.Properties.First(p => p.Key == "requireDevCredentials").Value = "false";
            manifest.ProvisioningUsername = awsAccessKey;
            manifest.ProvisioningPassword = awsSecretKey;
            var request = new AddonTestRequest { Manifest = manifest, DeveloperOptions = "" };

            var result = addon.Test(request);
            Assert.IsTrue(result.IsSuccess, result.EndUserMessage);
        }

        [Test]
        [Ignore]
        public static void TestSuccessfulProvisioning()
        {
            var addon = new Addon();
            var manifest = GetAWSAddonManifest();
            manifest.Properties.First(p => p.Key == "requireDevCredentials").Value = "false";
            manifest.ProvisioningUsername = awsAccessKey;
            manifest.ProvisioningPassword = awsSecretKey;

            string devOptions = "";
            devOptions += "Engine=sqlserver-se";
            devOptions += "&DbInstanceClass=db.m1.large";
            devOptions += "&DbInstanceIdentifier=db2";
            devOptions += "&DBAUsername=Abe";
            devOptions += "&DBAPassword=password";
            devOptions += "&AllocatedStorage=200";
            devOptions += "&LicenseModel=license-included";

            var request = new AddonProvisionRequest { Manifest = manifest, DeveloperOptions = devOptions };
            var result = addon.Provision(request);
            Assert.IsTrue(result.IsSuccess, result.EndUserMessage);
        }

        [Test]
        [Ignore]
        public static void TestSuccessfulDeProvisioning()
        {
            var addon = new Addon();
            var manifest = GetAWSAddonManifest();
            manifest.Properties.First(p => p.Key == "requireDevCredentials").Value = "false";
            manifest.ProvisioningUsername = awsAccessKey;
            manifest.ProvisioningPassword = awsSecretKey;

            var request = new AddonDeprovisionRequest { Manifest = manifest, DeveloperOptions = "DbInstanceIdentifier=db2" };

            var result = addon.Deprovision(request);
            Assert.IsTrue(result.IsSuccess, result.EndUserMessage);
        }

        private static AddonManifest GetAWSAddonManifest()
        {
            var manifest = new AddonManifest
            {
                Author = "Abraham Sultan",
                Vendor = "Apprenda Inc.",
                Version = "1.0.0.0",
                IsEnabled = true,
                Name = "AWS RDS",
                ProvisioningUsername = "Default",
                ProvisioningPassword = "Default",
                Properties = new List<AddonProperty>()
                      {
                          new AddonProperty(){Key = "requireDevCredentials", Value = "true"}
                      }
            };

            return manifest;
        }
    }
}
