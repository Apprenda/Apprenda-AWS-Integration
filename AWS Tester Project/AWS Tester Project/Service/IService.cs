using System.ServiceModel;

namespace Service
{
    [ServiceContract(Namespace = "Service", Name = "Service")]
    public interface IService
    {
        [OperationContract]
        DataTransferObject SomeMethod(DataTransferObject data);
    }
}
