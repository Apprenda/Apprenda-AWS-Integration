using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWS_DataPipeLine_AddOn
{
    class PipelineOptions
    {


        public string authenticationRegion { get; set; }

        public string authenticationServiceName { get; set; }

        public int buffersize { get; set; }

        public int connectionLimit { get; set; }

        public bool logmetrics { get; set; }

        public bool logresponse { get; set; }

        public int maxerrorretry { get; set; }

        public int maxidletime { get; set; }

        public long progressupdateinterval { get; set; }

        public System.Net.ICredentials proxycredentials { get; set; }

        public string proxyhost { get; set; }

        public int proxyport { get; set; }

        public bool readentirerespone { get; set; }

        public TimeSpan? readwritetimeout { get; set; }
    }
}
