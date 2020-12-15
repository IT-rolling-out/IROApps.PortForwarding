using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IRO.Mvc.Core.Dto;

namespace IROApps.PortForwarding.Dto
{
    public class SetResponseDto
    {
        public Guid Id { get; set; }

        public HttpContextInfo.ResponseInfo Resp { get; set; }
    }
}
