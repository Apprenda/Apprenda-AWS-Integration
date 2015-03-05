namespace Apprenda.SaaSGrid.Addons.AWS.EC2
{
    public class Addon : AddonBase
    {
        public override ProvisionAddOnResult Provision(AddonProvisionRequest request)
        {
            // load developer options and items from manifest
            var developerOptions = DeveloperOptions.Parse(request.DeveloperOptions);
            developerOptions.LoadItemsFromManifest(request.Manifest);

            var ec2Response = Ec2AmiFactory.StartServer(developerOptions);

            if(ec2Response.code = )
        }

        public override OperationResult Deprovision(AddonDeprovisionRequest request)
        {
            throw new System.NotImplementedException();
        }

        public override OperationResult Test(AddonTestRequest request)
        {
            throw new System.NotImplementedException();
        }
    }
}