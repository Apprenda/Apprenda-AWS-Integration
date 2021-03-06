﻿namespace Apprenda_AWS.Test
{
    using System.Collections.Generic;
    using System.Configuration;
    using Apprenda.SaaSGrid.Addons;
    using Apprenda.SaaSGrid.Addons.AWS.SNS;
    using NUnit.Framework;

    [TestFixture]
    public class SnsTestHarness
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
                    Key = "topicname",
                    Value = ConfigurationManager.AppSettings["SNSTopicName"]
                }
            };
            return paramConstructor;
        }

        private static AddonManifest SetupPropertiesAndParameters()
        {
            #region developer parameter definitions

            var plist = new List<DevParameter> { new DevParameter() { Key = "topicname", DisplayName = "Topic Name" } };

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
                Name = "SNS",

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
        public void ParseSnsParametersTest()
        {

        }

        [Test]
        public void ProvisionTestSameInstance()
        {
            this.ProvisionRequest = new AddonProvisionRequest { Manifest = SetupPropertiesAndParameters(), DeveloperParameters = SetUpParameters() };
            var addon = new SnsAddOn();
            var output = addon.Provision(this.ProvisionRequest);
            Assert.That(output, Is.TypeOf<ProvisionAddOnResult>());
            Assert.That(output.IsSuccess, Is.EqualTo(true));
            Assert.That(output.ConnectionData.Length, Is.GreaterThan(0));
            this.DeprovisionRequest = new AddonDeprovisionRequest
                                          {
                                              Manifest = SetupPropertiesAndParameters(),
                                              DeveloperParameters = SetUpParameters(),
                                              ConnectionData = output.ConnectionData
                                          };
            var deoutput = addon.Deprovision(this.DeprovisionRequest);
            Assert.That(deoutput, Is.TypeOf<ProvisionAddOnResult>());
            Assert.That(deoutput.IsSuccess, Is.EqualTo(true));
        }

        [Test]
        public void ProvisionTestMultipleInstances()
        {
            this.ProvisionRequest = new AddonProvisionRequest { Manifest = SetupPropertiesAndParameters(), DeveloperParameters = SetUpParameters() };
            var output = new SnsAddOn().Provision(this.ProvisionRequest);
            Assert.That(output, Is.TypeOf<ProvisionAddOnResult>());
            Assert.That(output.IsSuccess, Is.EqualTo(true));
            Assert.That(output.ConnectionData.Length, Is.GreaterThan(0));
            this.DeprovisionRequest = new AddonDeprovisionRequest
                                          {
                                              Manifest = SetupPropertiesAndParameters(),
                                              DeveloperParameters = SetUpParameters(),
                                              ConnectionData = output.ConnectionData
                                          };
            var deoutput = new SnsAddOn().Deprovision(this.DeprovisionRequest);
            Assert.That(deoutput, Is.TypeOf<ProvisionAddOnResult>());
            Assert.That(deoutput.IsSuccess, Is.EqualTo(true));
        }

        [Test]
        public void SNSSocTest()
        {
            this.TestRequest = new AddonTestRequest()
            {
                Manifest = SetupPropertiesAndParameters(),
                DeveloperParameters = SetUpParameters()
            };
            var output = new SnsAddOn().Test(this.TestRequest);
            Assert.That(output, Is.TypeOf<OperationResult>());
            Assert.That(output.IsSuccess, Is.EqualTo(true));
        }
    }
}
