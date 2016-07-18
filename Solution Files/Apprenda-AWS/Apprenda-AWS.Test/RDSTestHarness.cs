namespace Apprenda_AWS.Test
{
    using System.Collections.Generic;
    using System.Configuration;
    using Apprenda.SaaSGrid.Addons;
    using Apprenda.SaaSGrid.Addons.AWS.RDS;
    using NUnit.Framework;

    [TestFixture]
    // ReSharper disable once InconsistentNaming
    public class RDSTestHarness
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

        private static List<AddonParameter> SetUpParameters()
        {
            var paramConstructor = new List<AddonParameter>
            {
                new AddonParameter
                {
                    Key = "databasename",
                    Value = ConfigurationManager.AppSettings["databasename"]
                },
                new AddonParameter
                {
                    Key = "engine",
                    Value = ConfigurationManager.AppSettings["dbengine"]
                },
                new AddonParameter
                {
                    Key = "storage",
                    Value = ConfigurationManager.AppSettings["storage"]
                },

            };
            return paramConstructor;
        }

        private static AddonManifest SetupPropertiesAndParameters()
        {
            #region developer parameter definitions
            var plist = new List<DevParameter>
                            {
                                new DevParameter()
                                    {
                                        Key = "databasename",
                                        DisplayName = "Database Name"
                                    },
                                new DevParameter()
                                    {
                                        Key = "engine",
                                        DisplayName = "Database Engine"
                                    },
                                new DevParameter()
                                    {
                                        Key = "storage",
                                        DisplayName = "Storage Needed"
                                    }
                            };
            #endregion

            #region addon property definitions

            var props = new List<AddonProperty>
                            {
                                new AddonProperty { Key = "maxallocatedstorage", Value = "10" },
                                new AddonProperty { Key = "autominorversionupgrade", Value = "True" },
                                new AddonProperty { Key = "defaultaz", Value = "us-east-1" },
                                new AddonProperty { Key = "maxdbinstanceclass", Value = "db.t1.micro" },
                                new AddonProperty { Key = "oracleengineedition", Value = "se-1" },
                                new AddonProperty { Key = "sqlserverengineedition", Value = "web" },
                                new AddonProperty { Key = "multiaz", Value = "False" },
                                new AddonProperty { Key = "oracledbversion", Value = "" },
                                new AddonProperty { Key = "mssqldbversion", Value = "" },
                                new AddonProperty { Key = "mysqldbversion", Value = "" },
                                new AddonProperty { Key = "overrideport", Value = "False" },
                                new AddonProperty { Key = "backupretentionperiod", Value = "0" },
                                new AddonProperty { Key = "skipfinalsnapshot", Value = "True" },
                            
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
                Name = "RDS",

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
        public void ParseS3ParametersTest()
        {

        }

        [Test]
        public void ProvisionTest()
        {
            this.ProvisionRequest = new AddonProvisionRequest { Manifest = SetupPropertiesAndParameters(), DeveloperParameters = SetUpParameters() };
            var output = new RdsAddOn().Provision(this.ProvisionRequest);
            Assert.That(output, Is.TypeOf<ProvisionAddOnResult>());
            Assert.That(output.IsSuccess, Is.EqualTo(true));
            Assert.That(output.ConnectionData.Length, Is.GreaterThan(0));
        }

        [Test]
        public void DeProvisionTest()
        {
            this.ProvisionRequest = new AddonProvisionRequest { Manifest = SetupPropertiesAndParameters(), DeveloperParameters = SetUpParameters() };
            var provOutput = new RdsAddOn().Provision(this.ProvisionRequest);
            this.DeprovisionRequest = new AddonDeprovisionRequest
                                          {
                                              Manifest = SetupPropertiesAndParameters(),
                                              DeveloperParameters = SetUpParameters(),
                                              ConnectionData = provOutput.ConnectionData
                                          };
            // take the connection data from the provisioned request.
            var output = new RdsAddOn().Deprovision(this.DeprovisionRequest);
            Assert.That(output, Is.TypeOf<OperationResult>());
            Assert.That(output.IsSuccess, Is.EqualTo(true));
        }

        [Test]
        public void SocTest()
        {
            this.TestRequest = new AddonTestRequest()
            {
                Manifest = SetupPropertiesAndParameters(),
                DeveloperParameters = SetUpParameters()
            };
            var output = new RdsAddOn().Test(this.TestRequest);
            Assert.That(output, Is.TypeOf<OperationResult>());
            Assert.That(output.IsSuccess, Is.EqualTo(true));
        }
    }
}
