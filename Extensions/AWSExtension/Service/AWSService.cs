using System.IO;
using System.ServiceModel.Web;
using System.Text;
using Apprenda.SaaSGrid.Extensions;
using Apprenda.SaaSGrid.Extensions.DTO;

namespace Service
{
    public class AWSService : IDeveloperPortalExtensionService
    {
        public void OnDemotingVersion(ReadOnlyVersionDataDTO version, ApplicationVersionStageDTO proposedStage)
        {
            // ok, so on demotion: 
            // 1) we check to see if we *have* any running cloud services for this application, and whether or not to destroy them.
            // 2) if we do have cloud services and they should be deprovisioned, deprovision them. 
            
            if(CheckForAWSProvisionings() && CheckIfWeNeedToDestroy())
            {
                // ok, let's find out what cloud instances are provisioned to the application
                // this will be a database call to load them.

            }
            
        }

        private bool CheckIfWeNeedToDestroy()
        {
            throw new System.NotImplementedException();
        }

        private bool CheckForAWSProvisionings()
        {
            throw new System.NotImplementedException();
        }

        public void OnPromotingVersion(ReadOnlyVersionDataDTO version, ApplicationVersionStageDTO proposedStage)
        {
            throw new System.NotImplementedException();
        }


        public void OnVersionDemoted(ReadOnlyVersionDataDTO version, ApplicationVersionStageDTO previousStage, ApplicationVersionStageDTO proposedStage, PublishReportCardDataDTO reportCard)
        {
            throw new System.NotImplementedException();
        }

        public void OnVersionPromoted(ReadOnlyVersionDataDTO version, ApplicationVersionStageDTO previousStage, ApplicationVersionStageDTO proposedStage, PublishReportCardDataDTO reportCard)
        {
            throw new System.NotImplementedException();
        }
    }
}
