namespace Apprenda.SaaSGrid.Addons.AWS.EMR
{
    public class EmrConnectionInfo
    {
        public string EmrConnectionString { get; set; }

        public override string ToString()
        {
            return EmrConnectionString;
        }
    }
}