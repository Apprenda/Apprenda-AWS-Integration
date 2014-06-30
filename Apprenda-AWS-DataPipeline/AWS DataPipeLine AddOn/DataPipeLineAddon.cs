using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apprenda.SaaSGrid.Addons;
using Amazon.DataPipeline;
using Amazon.DataPipeline.Model;
using Amazon.Runtime;
using System.Threading;

namespace AWS_DataPipeLine_AddOn
{
    public class DataPipeLineAddon : AddonBase
    {
        public override OperationResult Deprovision(AddonDeprovisionRequest request)
        {
            throw new NotImplementedException();
        }

        public override ProvisionAddOnResult Provision(AddonProvisionRequest request)
        {
            // build the credentials, and then the configuration
            // ----------------------------------------------------
            string accessKey = "", secretKey = "";
            AWSCredentials  creds = new BasicAWSCredentials(accessKey, secretKey);
            AmazonDataPipelineConfig config = new AmazonDataPipelineConfig()
            {
                
            };
            AmazonDataPipelineClient datapipelineclient = new AmazonDataPipelineClient(creds, config);
            CreatePipelineRequest pipelinerequest = new CreatePipelineRequest()
            {
                Name = name,
                Description = description,
                UniqueId = "apprenda-" + Guid.NewGuid().ToString()
            };
            CreatePipelineResponse pipelineresponse = datapipelineclient.CreatePipeline(pipelinerequest);
            // wait for new pipeline id to complete
            // -------------------------------------
            while(pipelineresponse.PipelineId == null)
            {
                Thread.Sleep(100);
            }

            GetPipelineDefinitionResponse getpipelineresponse = datapipelineclient.GetPipelineDefinition(new GetPipelineDefinitionRequest()
                {
                    PipelineId = pipelineresponse.PipelineId
                });

            while(getpipelineresponse.PipelineObjects == null)
            {
                Thread.Sleep(100);
            }

            var properties = getpipelineresponse.PipelineObjects[0].Fields;


            ConnectionInfo info = new ConnectionInfo()
            {
                PipelineId = pipelineresponse.PipelineId,
                PipelineName = getpipelineresponse.PipelineObjects[0].Name
            };

            ConnectionInfo.MapToProperty(info, properties);
        }

        public override OperationResult Test(AddonTestRequest request)
        {
            // datapipeline --list-pipelines
        }



        private AmazonDataPipelineConfig createPipelineConfig(PipelineOptions options)
        {
            AmazonDataPipelineConfig mPipelineConfig = new AmazonDataPipelineConfig()
            {
                AuthenticationRegion = options.authenticationRegion,
                AuthenticationServiceName = options.authenticationServiceName,
                BufferSize = options.buffersize,
                ConnectionLimit = options.connectionLimit,
                LogMetrics = options.logmetrics,
                LogResponse = options.logresponse,
                MaxErrorRetry = options.maxerrorretry,
                MaxIdleTime = options.maxidletime,
                ProgressUpdateInterval = options.progressupdateinterval,
                ProxyCredentials = options.proxycredentials,
                ProxyHost = options.proxyhost,
                ProxyPort = options.proxyport,
                ReadEntireResponse = options.readentirerespone,
                ReadWriteTimeout = options.readwritetimeout,
                RegionEndpoint = options.regionendpoint,
                ServiceURL = options.serviceurl,
                SignatureMethod = options.signaturemethod,
                SignatureVersion = options.signatureversion,
                Timeout = options.timeout,
                UseHttp = options.usehttp,
                UseNagleAlgorithm = options.usenaglealgorithm,
                UserAgent = options.useragent
            };
            return mPipelineConfig;
        }
    }
}
