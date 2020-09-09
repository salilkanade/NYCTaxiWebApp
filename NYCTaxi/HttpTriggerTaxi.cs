using System;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using System.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Newtonsoft.Json;
using System.Net.Http;

namespace HTTPTrigger
{   
    public static class NegotiateFunction
    {
        [FunctionName("negotiate")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req,
                                        [SignalRConnectionInfo(HubName = "taxidata")] SignalRConnectionInfo info,
                                        ILogger log)
        {
            return info != null
                ? (ActionResult)new OkObjectResult(info)
                : new NotFoundObjectResult("Failed to load SignalR Info.");
        }
    }

    public static class MessageFunction
    {
        [FunctionName("message")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function)] HttpRequest req,
                                                    [SignalR(HubName = "taxidata")] IAsyncCollector<SignalRMessage> signalRMessages,
                                                    ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            log.LogInformation($"Request {requestBody}");
            if (string.IsNullOrEmpty(requestBody))
            {
                return new BadRequestObjectResult("Please pass a payload to broadcast in the request body.");
            }

            await signalRMessages.AddAsync(new SignalRMessage()
            {
                Target = "notify",
                Arguments = new object[] { requestBody }
            });

            return new OkResult();
        }
    }

}



