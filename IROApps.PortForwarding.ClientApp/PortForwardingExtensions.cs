using System.Net.Http;

namespace IROApps.PortForwarding.ClientApp
{
    public static class PortForwardingExtensions
    {
        public static HttpMethod HttpMethodFromString(string method)
        {
            var res = HttpMethod.Get;
            switch (method)
            {
                case "GET":
                    res = HttpMethod.Get;
                    break;

                case "POST":
                    res = HttpMethod.Post;
                    break;

                case "PUT":
                    res = HttpMethod.Put;
                    break;

                case "DELETE":
                    res = HttpMethod.Delete;
                    break;

                case "HEAD":
                    res = HttpMethod.Head;
                    break;

                case "PATCH":
                    res = HttpMethod.Patch;
                    break;

                case "OPTIONS":
                    res = HttpMethod.Options;
                    break;

                case "TRACE":
                    res = HttpMethod.Trace;
                    break;
            }
            return res;
        }
    }
}