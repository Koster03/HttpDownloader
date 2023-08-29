using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace HttpFileDownloader.Core
{
    public record ParsedURL(string protocol, IPAddress host, string path, string file);

    public static class HTTP
    {
        public static string CreateHTTPRequest(string method, ParsedURL url, Dictionary<string, string> headers)
        {
            StringBuilder sb = new StringBuilder($"{method} /{url.path} HTTP/1.1\r\nHost: {url.host}\r\n");

            foreach (var header in headers)
            {
                sb.Append(header.Key + ": " + header.Value + "\r\n");
            }

            sb.Append("\r\n");

            return sb.ToString();
        }

        public static int ReadAnswerCode(string header)
        {
            Regex reg = new Regex(@"HTTP\/1.[10] (\d*)");
            return int.Parse(reg.Match(header).Groups[1].ToString());
        }

        public static string GetHeadInfo(string header, string HTTPResponse)
        {
            Regex reg = new Regex($@"{header}: (.*)\r\n");
            return reg.Match(HTTPResponse).Groups[1].ToString();
        }

        public static ParsedURL ParseURL(string url)
        {
            var splitOne = url.Split("//");
            var splitSecondPart = splitOne.Last().Split('/');

            var protocolType = splitOne.First().Remove(splitOne.First().Length - 1);

            var ipAddress = NetUtil.ResolveIpAddress(splitSecondPart[0]);

            var path = string.Join('/', splitSecondPart
                .Skip(1)
                .Take(splitSecondPart.Length - 1));

            var fileName = splitSecondPart.Last();

            return new ParsedURL(protocolType, ipAddress, path, fileName);
        }
    }
}
