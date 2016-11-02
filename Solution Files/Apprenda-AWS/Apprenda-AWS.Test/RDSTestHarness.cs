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

        private static IEnumerable<AddonParameter> SetUpParameters()
        {
            var paramConstructor = new List<AddonParameter>
            {
                new AddonParameter
                {
                    Key = "dbname",
                    Value = ConfigurationManager.AppSettings["RDSDBName"]
                },
                new AddonParameter
                {
                    Key = "engine",
                    Value = ConfigurationManager.AppSettings["RDSEngine"]
                },
                new AddonParameter
                {
                    Key = "allocatedstorage",
                    Value = ConfigurationManager.AppSettings["RDSAllocatedStorage"]
                },
                new AddonParameter
                {
                    Key = "dbausername",
                    Value = ConfigurationManager.AppSettings["RDSDBAUsername"]
                },
                new AddonParameter
                {
                    Key = "dbapassword",
                    Value = ConfigurationManager.AppSettings["RDSDBAPassword"]
                },
                new AddonParameter
                {
                    Key = "dbinstanceidentifier",
                    Value = ConfigurationManager.AppSettings["RDSDBInstanceIdentifier"]
                }
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
                                        Key = "allocatedstorage",
                                        DisplayName = "Storage Needed"
                                    },
                                new DevParameter()
                                    {
                                        Key = "dbausername",
                                        DisplayName = "DBA Username"
                                    },
                                new DevParameter()
                                    {
                                        Key = "dbapassword",
                                        DisplayName = "DBAPassword"
                                    },
                                new DevParameter()
                                    {
                                        Key = "dbinstanceidentifier",
                                    }
                            };
            #endregion

            #region addon property definitions

            var props = new List<AddonProperty>
                            {
                                new AddonProperty { Key = "maxallocatedstorage", Value = ConfigurationManager.AppSettings["RDSMaxAllocatedStorage"]},
                                new AddonProperty { Key = "autominorversionupgrade", Value = ConfigurationManager.AppSettings["RDSAutoMinorVersionUpgrade"] },
                                new AddonProperty { Key = "availabilityzone", Value = ConfigurationManager.AppSettings["RDSAvailabilityZone"] },
                                new AddonProperty { Key = "maxdbinstanceclass", Value = ConfigurationManager.AppSettings["RDSMaxDBInstanceClass"] },
                                new AddonProperty { Key = "oracleengineedition", Value = ConfigurationManager.AppSettings["RDSOracleEngineEdition"] },
                                new AddonProperty { Key = "sqlserverengineedition", Value = ConfigurationManager.AppSettings["RDSSqlServerEngineEdition"] },
                                new AddonProperty { Key = "multiaz", Value = ConfigurationManager.AppSettings["RDSMultiAZ"] },
                                new AddonProperty { Key = "oracledbversion", Value = ConfigurationManager.AppSettings["RDSOracleDBVersion"] },
                                new AddonProperty { Key = "sqlserverdbversion", Value = ConfigurationManager.AppSettings["RDSSqlServerDBVersion"] },
                                new AddonProperty { Key = "mysqldbversion", Value = ConfigurationManager.AppSettings["RDSMySQLDBVersion"] },
                                new AddonProperty { Key = "port", Value = ConfigurationManager.AppSettings["RDSPort"] },
                                new AddonProperty { Key = "backupretentionperiod", Value = ConfigurationManager.AppSettings["RDSBackupRetentionPeriod"] },
                                new AddonProperty { Key = "skipfinalsnapshot", Value = ConfigurationManager.AppSettings["RDSSkipFinalSnapshot"] },
                                new AddonProperty { Key = "publiclyaccessible", Value = ConfigurationManager.AppSettings["RDSPubliclyAccessible"] },
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
        public void ProvisionRDSTest()
        {
            this.ProvisionRequest = new AddonProvisionRequest { Manifest = SetupPropertiesAndParameters(), DeveloperParameters = SetUpParameters() };
            var output = new RdsAddOn().Provision(this.ProvisionRequest);
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
            var deOutput = new RdsAddOn().Deprovision(this.DeprovisionRequest);
            Assert.That(deOutput, Is.TypeOf<OperationResult>());
            Assert.That(deOutput.IsSuccess, Is.EqualTo(true));
        }

        [Test]
        public void RDSSocTest()
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
