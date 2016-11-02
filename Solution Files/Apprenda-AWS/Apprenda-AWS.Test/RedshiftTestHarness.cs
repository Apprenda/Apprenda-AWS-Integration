namespace Apprenda_AWS.Test
{
    using System.Collections.Generic;
    using System.Configuration;
    using Apprenda.SaaSGrid.Addons;
    using Apprenda.SaaSGrid.Addons.AWS.Redshift;
    using NUnit.Framework;

    [TestFixture]
    public class RedshiftTestHarness
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
                new AddonParameter { Key = "ClusterIdentifier", Value = ConfigurationManager.AppSettings["RedshiftDevClusterIdentifier"]},
                new AddonParameter { Key = "DBName", Value = ConfigurationManager.AppSettings["RedshiftDevDBName"]},
                //new AddonParameter { Key = "AllocatedStorage", Value = ConfigurationManager.AppSettings["RedshiftDevAllocatedStorage"]},
                new AddonParameter { Key = "NodeCount", Value = ConfigurationManager.AppSettings["RedshiftDevNodeCount"]},
                new AddonParameter { Key = "ClusterType", Value = ConfigurationManager.AppSettings["RedshiftDevClusterType"] }
            };
            return paramConstructor;
        }

        private static AddonManifest SetupPropertiesAndParameters()
        {
            #region developer parameter definitions

            var plist = new List<DevParameter>
                            {
                                new DevParameter() { Key = "ClusterIdentifer"},
                                new DevParameter() { Key = "DBName"},
                                new DevParameter() { Key = "ClusterType" }
                            };

            #endregion

            #region addon property definitions

            var props = new List<AddonProperty>
                            {
                                new AddonProperty { Key = "AllowVersionUpgrade", Value = ConfigurationManager.AppSettings["RedshiftAllowVersionUpgrade"] },
                                new AddonProperty { Key = "AutomatedSnapshotRetentionPeriod", Value = ConfigurationManager.AppSettings["RedshiftAutomatedSnapshotRetentionPeriod"] },
                                new AddonProperty { Key = "SkipFinalSnapshot", Value=ConfigurationManager.AppSettings["RedshiftSkipFinalSnapshot"] },
                                new AddonProperty { Key = "DBName", Value = ConfigurationManager.AppSettings["RedshiftDBName"] },
                                new AddonProperty { Key = "Encrypted", Value = ConfigurationManager.AppSettings["RedshiftEncrypted"] },
                                new AddonProperty { Key = "MasterUserName", Value = ConfigurationManager.AppSettings["RedshiftMasterUsername"] },
                                new AddonProperty { Key = "MasterUserPassword", Value = ConfigurationManager.AppSettings["RedshiftMasterUserPassword"] },
                                new AddonProperty { Key = "NodeType", Value = ConfigurationManager.AppSettings["RedshiftNodeType"] },
                                new AddonProperty { Key = "MaxNumberOfNodes", Value = ConfigurationManager.AppSettings["RedshiftMaxNumberOfNodes"] },
                                new AddonProperty { Key = "Port", Value = ConfigurationManager.AppSettings["RedshiftDefaultPort"] },
                                new AddonProperty { Key = "PubliclyAccessible", Value = ConfigurationManager.AppSettings["RedshiftPubliclyAccessible"] },
                            };

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
                Name = "Redshift",

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
        public void ProvisionRedshiftTest()
        {
            this.ProvisionRequest = new AddonProvisionRequest { Manifest = SetupPropertiesAndParameters(), DeveloperParameters = SetUpParameters() };
            var output = new RedshiftAddOn().Provision(this.ProvisionRequest);
            Assert.That(output, Is.TypeOf<ProvisionAddOnResult>());
            Assert.That(output.IsSuccess, Is.EqualTo(true));
            Assert.That(output.ConnectionData.Length, Is.GreaterThan(0));
            this.ProvisionRequest = new AddonProvisionRequest { Manifest = SetupPropertiesAndParameters(), DeveloperParameters = SetUpParameters() };
            this.DeprovisionRequest = new AddonDeprovisionRequest
                                          {
                                              Manifest = SetupPropertiesAndParameters(),
                                              DeveloperParameters = SetUpParameters(),
                                              ConnectionData = output.ConnectionData
                                          };
            var deOutput = new RedshiftAddOn().Deprovision(this.DeprovisionRequest);
            Assert.That(deOutput, Is.TypeOf<ProvisionAddOnResult>());
            Assert.That(deOutput.IsSuccess, Is.EqualTo(true));
        }

        [Test]
        public void RedshiftSocTest()
        {
            this.TestRequest = new AddonTestRequest()
            {
                Manifest = SetupPropertiesAndParameters(),
                DeveloperParameters = SetUpParameters()
            };
            var output = new RedshiftAddOn().Test(this.TestRequest);
            Assert.That(output, Is.TypeOf<OperationResult>());
            Assert.That(output.IsSuccess, Is.EqualTo(true));
        }
    }
}
