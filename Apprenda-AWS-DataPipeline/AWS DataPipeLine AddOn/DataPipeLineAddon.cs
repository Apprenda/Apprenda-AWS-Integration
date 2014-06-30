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
using AWS.Core;

namespace AWS_DataPipeLine_AddOn
{
    public class DataPipeLineAddOn : AddonBase
    {
        public override OperationResult Deprovision(AddonDeprovisionRequest request)
        {
            string connectionData = request.ConnectionData;
            // changing to overloaded constructor - 5/22/14
            var deprovisionResult = new ProvisionAddOnResult(connectionData);
            AddonManifest manifest = request.Manifest;
            string devOptions = request.DeveloperOptions;

            try
            {
                
                var conInfo = ConnectionInfo.Parse(connectionData);
                var developerOptions = PipelineOptions.Parse(devOptions, manifest);
                AmazonDataPipelineConfig config = createPipelineConfig(developerOptions);
                AmazonDataPipelineClient client = new AmazonDataPipelineClient(developerOptions.AccessKey, developerOptions.SecretAccessKey, config);

                var response =
                    client.DeletePipeline(new DeletePipelineRequest()
                    {
                        PipelineId = conInfo.PipelineId
                    });
                // 5/22/14 fixing amazon aws deprecation
                if (response.HttpStatusCode != null)
                {
                    do
                    {
                        var verificationResponse = client.DescribePipelines(new DescribePipelinesRequest()
                        {
                            PipelineIds = new List<String>() { conInfo.PipelineId }
                        });
                        // 5/22/14 fixing amazaon aws deprecration
                        if (!verificationResponse.PipelineDescriptionList.Any())
                        {
                            deprovisionResult.IsSuccess = true;
                            break;
                        }
                        Thread.Sleep(TimeSpan.FromSeconds(10d));

                    } while (true);
                }
            }
            catch (PipelineNotFoundException)
            {
                deprovisionResult.IsSuccess = true;
            }
            catch (Exception e)
            {
                deprovisionResult.EndUserMessage = e.Message;
            }

            return deprovisionResult;
        }

        public override ProvisionAddOnResult Provision(AddonProvisionRequest request)
        {
            // build the credentials, and then the configuration
            // ----------------------------------------------------
            string accessKey = "", secretKey = "";
            AWSCredentials  creds = new BasicAWSCredentials(accessKey, secretKey);
            PipelineOptions options = PipelineOptions.Parse(request.DeveloperOptions, request.Manifest);
            AddonManifest manifest = request.Manifest;

            ProvisionAddOnResult result = new ProvisionAddOnResult()
            {
                IsSuccess = false
            };
            AmazonDataPipelineConfig config = createPipelineConfig(options);
            AmazonDataPipelineClient datapipelineclient = new AmazonDataPipelineClient(creds, config);
            CreatePipelineRequest pipelinerequest = new CreatePipelineRequest()
            {
                Name = options.PipelineName,
                Description = options.PipelineDesc,
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

            ConnectionInfo.MapToProperty(info, properties, out info);
            result.ConnectionData = info.ToString();
            result.IsSuccess = true;
            result.EndUserMessage = "Successfully provisioned.";
            return result;
        }

        public override OperationResult Test(AddonTestRequest request)
        {
            OperationResult testresult = new OperationResult();
            testresult.IsSuccess = false;
            testresult.EndUserMessage = "Staring testing... \n";
            // datapipeline --list-pipelines
            AddonManifest manifest = request.Manifest;
            PipelineOptions options = PipelineOptions.Parse(request.DeveloperOptions, manifest);
            // Provision Request will be used for both actions
            AddonProvisionRequest addonrequest = new AddonProvisionRequest()
                {
                    DeveloperOptions = request.DeveloperOptions,
                    Manifest = request.Manifest
                };
            ProvisionAddOnResult provisionresult = Provision(addonrequest);
            if(!provisionresult.IsSuccess)
            {
                // stop test.
                testresult.EndUserMessage += @"Provision failed. Check your settings and try again. \n";
                return testresult;
            }
            AddonDeprovisionRequest deprovrequest = new AddonDeprovisionRequest()
            {
                ConnectionData = provisionresult.ConnectionData,
                DeveloperOptions = request.DeveloperOptions,
                Manifest = request.Manifest
            };
            OperationResult deprovsionResult = Deprovision(deprovrequest);
            if(!deprovsionResult.IsSuccess)
            {
                testresult.EndUserMessage += @"Deprovision failed. Manually remove your provisioned resource via the AWS console, fix your code, and try again. \n";
                return testresult;
            }

            testresult.EndUserMessage += @"Test successful!";
            testresult.IsSuccess = true;
            return testresult;
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
