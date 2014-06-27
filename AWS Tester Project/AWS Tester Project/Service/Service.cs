using System.IO;
using System.ServiceModel.Web;
using System.Text;

namespace Service
{
    public class Service : IService, ISilverlightClientAccessPolicy
    {
        public DataTransferObject SomeMethod(DataTransferObject data)
        {
            return data;
        }

        public Stream GetClientAccessPolicy()
        {
            const string result =
                @"<?xml version=""1.0"" encoding=""utf-8""?>
                                        <access-policy>
                                            <cross-domain-access>
                                                <policy>
                                                    <allow-from http-request-headers=""*"">
                                                        <domain uri=""*""/>
                                                    </allow-from>
                                                    <grant-to>
                                                        <resource path=""/"" include-subpaths=""true""/>
                                                    </grant-to>
                                                </policy>
                                            </cross-domain-access>
                                        </access-policy>";

            if (WebOperationContext.Current != null)
            {
                WebOperationContext.Current.OutgoingResponse.ContentType = "application/xml";
            }

            return new MemoryStream(Encoding.UTF8.GetBytes(result));
        }
    }
}
