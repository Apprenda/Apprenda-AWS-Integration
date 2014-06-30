using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apprenda.SaaSGrid.Addons;

namespace Amazon_Glacier_AddOn
{
    public class GlacierAddon : AddonBase 
    {
        public override OperationResult Deprovision(AddonDeprovisionRequest request)
        {
            throw new NotImplementedException();
        }

        public override ProvisionAddOnResult Provision(AddonProvisionRequest request)
        {
            throw new NotImplementedException();
        }

        public override OperationResult Test(AddonTestRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
