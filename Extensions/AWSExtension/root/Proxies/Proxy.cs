using root.Contracts;
using System.ServiceModel;

namespace root.Proxies
{
    internal class Proxy : ClientBase<IService>, IService
    {
        public DataTransferObject SomeMethod(DataTransferObject data)
        {
            return base.Channel.SomeMethod(data);
        }
    }
}
