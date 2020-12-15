using System;
using IRO.Mvc.Core.Dto;

namespace IROApps.PortForwarding.ServerApp.Dto
{
    public class SetResponseDto
    {
        public Guid Id { get; set; }

        public HttpContextInfo.ResponseInfo Resp { get; set; }
    }
}
