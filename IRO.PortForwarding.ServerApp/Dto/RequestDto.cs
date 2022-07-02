using System;
using IRO.Mvc.Core.Dto;

namespace IRO.PortForwarding.ServerApp.Dto
{
    public class RequestDto
    {
        public Guid Id { get; set; }

        public HttpContextInfo.RequestInfo Req { get; set; }
    }
}