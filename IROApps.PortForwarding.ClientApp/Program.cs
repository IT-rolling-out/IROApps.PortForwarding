using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using IRO.Mvc.Core.Dto;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace IROApps.PortForwarding.ClientApp
{
    class Program
    {
        static string _setPortsEndpoint;
        static string _getPendingRequestsEndpoint;
        static HttpClient _client;
        static string _addressTo;

        static void Main(string[] args)
        {
            var commandObj = Args.Configuration.Configure<CommandObject>().CreateAndBind(args);
            if (commandObj.AddressTo.EndsWith("/"))
            {
                commandObj.AddressTo = commandObj.AddressTo.Remove(commandObj.Server.Length - 1);
            }

            if (commandObj.Server.EndsWith("/"))
            {
                commandObj.Server = commandObj.Server.Remove(commandObj.Server.Length - 1);
            }

            _getPendingRequestsEndpoint =
                $"{commandObj.Server}/portforwarding/getPendingRequests?adminkey={commandObj.AdminKey}";
            _setPortsEndpoint = $"{commandObj.Server}/portforwarding/setResponse?adminkey={commandObj.AdminKey}";
            var clientHandler = new HttpClientHandler();
            clientHandler.AllowAutoRedirect = false;
            //clientHandler.MaxAutomaticRedirections = 10;
            _client = new HttpClient(clientHandler);



            _addressTo = commandObj.AddressTo;

            Console.WriteLine($"Listening.\n  Address to: {_addressTo}\n  Server: {commandObj.Server}.");

            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        await CheckRequests();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error resolving pending requests.\n{ex}");
                    }

                    await Task.Delay(100);
                }
            });
            while (true)
            {
                Console.ReadLine();
            }
        }

        static async Task CheckRequests()
        {
            var resp = await _client.GetAsync(_getPendingRequestsEndpoint);
            var text = await resp.Content.ReadAsStringAsync();
            var pendingRequests = JsonConvert.DeserializeObject<Dictionary<Guid, HttpContextInfo.RequestInfo>>(text);
            if (pendingRequests == null)
            {
                pendingRequests = new Dictionary<Guid, HttpContextInfo.RequestInfo>();
            }

            foreach (var pair in pendingRequests)
            {
                try
                {
                    var respInfo = await HandlePendingRequest(pair.Value);
                    await SendResponseOfPending(pair.Key, respInfo);
                    await Task.Delay(10);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in request {pair.Key}.\n{ex}");
                }

            }

        }

        static async Task<HttpContextInfo.ResponseInfo> HandlePendingRequest(HttpContextInfo.RequestInfo req)
        {
            var reqMsg = new HttpRequestMessage();
            reqMsg.Method = PortForwardingExtensions.HttpMethodFromString(req.Method);
            var reqUrl = $"{_addressTo}{req.Path}";
            if (req.QueryParameters.Count > 0)
            {
                reqUrl += "?";
                foreach (var queryItem in req.QueryParameters)
                {
                    reqUrl += $"{queryItem.Key}={queryItem.Value.First()}";
                }
            }

            reqMsg.RequestUri = new Uri(reqUrl);
            if (!string.IsNullOrWhiteSpace(req.BodyText))
            {
                var content = new StringContent(req.BodyText);
                foreach (var header in req.Headers)
                {
                    content.Headers.TryAddWithoutValidation(header.Key,  header.Value);
                }
                content.Headers.ContentType.MediaType = req.ContentType;
                reqMsg.Content = content;
            }

            var respMsg = await _client.SendAsync(reqMsg);
            var respInfo = new HttpContextInfo.ResponseInfo();
            respInfo.ContentType = respMsg.Content.Headers.ContentType?.MediaType;
            respInfo.ContentLength = respMsg.Content.Headers.ContentLength ?? 0;
            respInfo.Headers = new Dictionary<string, IEnumerable<string>>();
            foreach (var header in respMsg.Headers)
            {
                respInfo.Headers[header.Key] = header.Value;
            }
            try
            {
                respInfo.BodyText = await respMsg.Content.ReadAsStringAsync();
            }
            catch
            {
                // ignored
            }

            respInfo.StatusCode = (int)respMsg.StatusCode;

            return respInfo;
        }

        static async Task SendResponseOfPending(Guid id, HttpContextInfo.ResponseInfo resp)
        {
            var reqMsg = new HttpRequestMessage();
            reqMsg.Method = HttpMethod.Post;
            reqMsg.RequestUri = new Uri(_setPortsEndpoint);
            var dto = new SetResponseDto()
            {
                Id = id,
                Resp = resp
            };
            var reqStr = JsonConvert.SerializeObject(dto);
            reqMsg.Content = new StringContent(reqStr);
            reqMsg.Content.Headers.ContentType.MediaType = "application/json";
            await _client.SendAsync(reqMsg);
        }
    }
}
