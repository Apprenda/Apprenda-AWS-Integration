using System.ServiceModel;

namespace root.Contracts
{
    [ServiceContract(Namespace = "Service", Name = "Service")]
    public interface IService
    {
        [OperationContract]
        DataTransferObject SomeMethod(DataTransferObject data);
    }
}
