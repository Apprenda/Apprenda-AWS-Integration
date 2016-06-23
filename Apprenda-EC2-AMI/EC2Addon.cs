namespace Apprenda.SaaSGrid.Addons.AWS.EC2
{
    public class EC2Addon : AddonBase
    {
        public override ProvisionAddOnResult Provision(AddonProvisionRequest request)
        {
            // TODO
            // load developer options and items from manifest
            var developerOptions = DeveloperOptions.Parse(request.DeveloperOptions);
            developerOptions.LoadItemsFromManifest(request.Manifest);

            var ec2Response = Ec2AmiFactory.StartServer(developerOptions);
            return null;
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