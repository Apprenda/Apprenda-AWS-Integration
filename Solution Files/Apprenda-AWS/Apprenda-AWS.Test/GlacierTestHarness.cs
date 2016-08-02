namespace Apprenda_AWS.Test
{
    using System.Collections.Generic;
    using System.Configuration;
    using Apprenda.SaaSGrid.Addons;
    using Apprenda.SaaSGrid.Addons.AWS.Glacier;
    using NUnit.Framework;

    [TestFixture]
    public class GlacierTestHarness
    {
        private AddonProvisionRequest ProvisionRequest { get; set; }
        private AddonDeprovisionRequest DeprovisionRequest { get; set; }
        private AddonTestRequest TestRequest { get; set; }

        [SetUp]
        public void SetupManifest()
        {
            this.ProvisionRequest = new AddonProvisionRequest
            {
                Manifest = SetupPropertiesAndParameters(),
                DeveloperParameters = SetUpParameters()
            };
            this.DeprovisionRequest = new AddonDeprovisionRequest { Manifest = SetupPropertiesAndParameters() };
            this.TestRequest = new AddonTestRequest { Manifest = SetupPropertiesAndParameters() };
        }

        private static IEnumerable<AddonParameter> SetUpParameters()
        {
            var paramConstructor = new List<AddonParameter>
            {
                new AddonParameter
                {
                    Key = "vaultname",
                    Value = ConfigurationManager.AppSettings["GlacierVaultName"]
                }
            };
            return paramConstructor;
        }

        private static AddonManifest SetupPropertiesAndParameters()
        {
            #region developer parameter definitions

            var plist = new List<DevParameter>
                            {
                                new DevParameter() { Key = "vaultname", DisplayName = "Vault Name" }
                            };

            #endregion

            #region addon property definitions

            var props = new List<AddonProperty>();

            #endregion
            
            var manifest = new AddonManifest
            {
                AllowUserDefinedParameters = true,
                Author = "Chris Dutra",
                DeploymentNotes = "",
                Description = "",
                DeveloperHelp = "",
                IsEnabled = true,
                ManifestVersionString = "2.0",
                Name = "Glacier",

                // we'll handle parameters below.
                Parameters = new ParameterList
                {
                    AllowUserDefinedParameters = "true",
                    // ReSharper disable once CoVariantArrayConversion
                    Items = plist.ToArray()
                },
                Properties = props,
                ProvisioningLocation = ConfigurationManager.AppSettings["ProvisioningLocation"],
                ProvisioningPassword = ConfigurationManager.AppSettings["ProvisioningPassword"],
                ProvisioningPasswordHasValue = false,
                ProvisioningUsername = ConfigurationManager.AppSettings["ProvisioningUsername"],
                Vendor = "Apprenda",
                Version = "2.1"
            };
            return manifest;
        }

        private class DevParameter : IAddOnParameterDefinition
        {
            public string Key { get; set; }

            public string DisplayName { get; set; }

            public string Description { get; set; }

            public bool IsEncrypted { get; set; }

            public bool IsRequired { get; set; }

            public bool HasValue { get; set; }

            public string DefaultValue { get; set; }
        }


        [Test]
        public void ProvisionGlacierTest()
        {
            this.ProvisionRequest = new AddonProvisionRequest { Manifest = SetupPropertiesAndParameters(), DeveloperParameters = SetUpParameters() };
            var output = new GlacierAddon().Provision(this.ProvisionRequest);
            Assert.That(output, Is.TypeOf<ProvisionAddOnResult>());
            Assert.That(output.IsSuccess, Is.EqualTo(true));
            Assert.That(output.ConnectionData.Length, Is.GreaterThan(0));
            this.DeprovisionRequest = new AddonDeprovisionRequest
                                          {
                                              Manifest = SetupPropertiesAndParameters(),
                                              DeveloperParameters = SetUpParameters(),
                                              ConnectionData = output.ConnectionData
                                          };
            // take the connection data from the provisioned request.
            var de_output = new GlacierAddon().Deprovision(this.DeprovisionRequest);
            Assert.That(de_output, Is.TypeOf<OperationResult>());
            Assert.That(de_output.IsSuccess, Is.EqualTo(true));
        }

        [Test]
        public void GlacierSocTest()
        {
            this.TestRequest = new AddonTestRequest()
            {
                Manifest = SetupPropertiesAndParameters(),
                DeveloperParameters = SetUpParameters()
            };
            var output = new GlacierAddon().Test(this.TestRequest);
            Assert.That(output, Is.TypeOf<OperationResult>());
            Assert.That(output.IsSuccess, Is.EqualTo(true));
        }
    }
}
