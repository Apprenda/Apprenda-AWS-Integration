using System.Runtime.Serialization;

namespace Service
{
    [DataContract(Namespace = "Service", Name = "DataTransfer")]
    public class DataTransferObject
    {
        [DataMember]
        public int Result { get; set; }
    }
}
