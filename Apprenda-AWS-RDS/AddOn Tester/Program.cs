using Amazon_RDS_AddOn;
using Apprenda.SaaSGrid.Addons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AddOn_Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            var addon = new Addon();
            var manifest = new AddonManifest
                {
                  Author  = "Abraham Sultan",
                  Vendor = "Apprenda Inc.",
                  Version = "1.0.0.0",
                  IsEnabled = true,
                  Name = "AWS RDS",
                  Properties = new List<AddonProperty>()
                      {
                          new AddonProperty(){Key = "requireDevCredentials", Value = "true"}
                      }
                };
            
            var request = new AddonTestRequest { Manifest = manifest, DeveloperOptions = "" };
            addon.Test(request);
        }
    }
}
