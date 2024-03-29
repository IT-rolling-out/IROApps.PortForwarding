﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IRO.Mvc.Core;
using IRO.Mvc.Core.Dto;
using IRO.PortForwarding.ServerApp.Dto;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;

namespace IRO.PortForwarding.ServerApp.Service
{
    public static class CatchRequestsMiddleware
    {
        public static IDictionary<Guid, HttpContextInfo.ResponseInfo> PendingRespDict { get; } =
            new ConcurrentDictionary<Guid, HttpContextInfo.ResponseInfo>();

        public static IDictionary<Guid, HttpContextInfo.RequestInfo> PendingReqDict { get; } =
            new ConcurrentDictionary<Guid, HttpContextInfo.RequestInfo>();


        public static void UseCatchRequestsMiddleware(this IApplicationBuilder app)
        {

            app.Use(async (ctx, next) =>
            {
                try
                {
                    var contextInfo = await ctx.ResolveInfo();
                    var copy = JsonConvert.DeserializeObject<HttpContextInfo>(JsonConvert.SerializeObject(contextInfo));
                    if (contextInfo.Request.Path.StartsWith("/portforwarding/getPendingRequests"))
                    {
                        await GetPendingRequests(ctx);
                        return;
                    }
                    if (contextInfo.Request.Path.StartsWith("/portforwarding/setResponse"))
                    {
                        await SetResponse(ctx);
                        return;
                    }

                    var reqId = Guid.NewGuid();
                    PendingReqDict[reqId] = contextInfo.Request;

                    var hub = ctx.RequestServices.GetRequiredService<IHubContext<PendingRequestsHub>>();
                    await hub.Clients.All.SendAsync("PendingRequest", new RequestDto()
                    {
                        Id = reqId,
                        Req = contextInfo.Request
                    });

                    HttpContextInfo.ResponseInfo respInfo = null;
                    var startWaitAt = DateTime.UtcNow;
                    while (respInfo == null && DateTime.UtcNow - startWaitAt < AppSettings.RequestExpireTime)
                    {
                        PendingRespDict.TryGetValue(reqId, out respInfo);
                        await Task.Delay(10);
                    }
                    PendingReqDict.Remove(reqId);
                    PendingRespDict.Remove(reqId);
                    if (respInfo == null)
                    {
                        ctx.Response.StatusCode = 501;
                        await ctx.Response.WriteAsync("Response from client timeout.");
                    }
                    else
                    {
                        ctx.Response.StatusCode = respInfo.StatusCode;
                        if (respInfo.Headers != null)
                        {
                            foreach (var pair in respInfo.Headers)
                            {
                                if (pair.Key == "Transfer-Encoding")
                                    continue;
                                if (pair.Key == "Location")
                                {
                                    ctx.Response.Redirect(pair.Value.First());
                                    continue;
                                }
                                var sv = new StringValues(pair.Value.ToArray());
                                ctx.Response.Headers[pair.Key] = sv;
                            }
                        }
                        if (respInfo.ContentType != null)
                            ctx.Response.ContentType = respInfo.ContentType;
                        if (respInfo.StatusCode != 0)
                            ctx.Response.StatusCode = respInfo.StatusCode;
                        if (respInfo.BodyText != null)
                            await ctx.Response.WriteAsync(respInfo.BodyText);
                    }

                }
                catch (Exception ex)
                {
                    ctx.Response.StatusCode = 500;
                    await ctx.Response.WriteAsync($"Port forwarding server internal error.\n{ex}");
                    await next();
                }
            });

        }

        static async Task GetPendingRequests(HttpContext ctx)
        {
            var adminkey = ctx.Request.Query["adminkey"];
            if (adminkey != AppSettings.ADMIN_KEY)
            {
                throw new Exception("Admin key is wrong.");
            }
            var respJson = JsonConvert.SerializeObject(PendingReqDict);
            ctx.Response.StatusCode = 200;
            ctx.Response.ContentType = "application/json";
            await ctx.Response.WriteAsync(respJson);
        }

        static async Task SetResponse(HttpContext ctx)
        {
            var adminkey = ctx.Request.Query["adminkey"];
            if (adminkey != AppSettings.ADMIN_KEY)
            {
                throw new Exception("Admin key is wrong.");
            }
            var jsonStr = await ctx.GetRequestBodyText();

            var dto = JsonConvert.DeserializeObject<SetResponseDto>(jsonStr);
            var req = PendingReqDict[dto.Id];
            PendingRespDict[dto.Id] = dto.Resp;
            ctx.Response.StatusCode = 200;
        }

    }


}
