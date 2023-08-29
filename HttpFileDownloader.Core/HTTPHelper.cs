using System.Net;
using System.Net.Sockets;
using System.Text;

namespace HttpFileDownloader.Core
{
    public class HTTPHelper
    {
        public Socket ConnectToURL(ParsedURL url)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var endPoint = new IPEndPoint(url.host, 80);
            socket.Connect(endPoint);

            return socket;
        }

        public string GetHead(string HttpRequest, Socket socket)
        {
            socket.Send(Encoding.ASCII.GetBytes(HttpRequest));

            string header = this.ReadHeader(socket);

            int answerCode = HTTP.ReadAnswerCode(header);

            if (answerCode < 200 || answerCode > 299)
            {
                throw new Exception($"Http response return code: {answerCode}");
            }

            return header;
            //return HTTP.GetInfoFromHTTPResponse(Head, header);
        }

        public void GetBody(string HttpRequest, Socket socket, Region region, DownloadMap downloadMap, string filePath)
        {
            socket.Send(Encoding.ASCII.GetBytes(HttpRequest));

            string header = this.ReadHeader(socket);

            int answerCode = HTTP.ReadAnswerCode(header);

            if (answerCode < 200 || answerCode > 299)
            {
                throw new Exception($"Answer code is {answerCode}");
            }

            long gettingBytes = 0;
            //var addfa = new List<byte>();
            var startPos = region.start;

            while (gettingBytes < region.Size)
            {
                var count = socket.Available;

                if (count == 0)
                {
                    continue;
                }

                byte[] buffer = new byte[count];
                socket.Receive(buffer, 0, count, 0);

                downloadMap.MarkRegion(startPos, count, RegionState.Downloaded);

                FileWriter.Write(buffer, (int)startPos, filePath);

                startPos += count;
                gettingBytes += count;
                //addfa.AddRange(buffer);
            }
        }

        private string ReadHeader(Socket socket)
        {
            var sb = new StringBuilder();

            do
            {
                byte[] buffer = new byte[1];
                socket.Receive(buffer, 0, 1, 0);
                sb.Append(Encoding.ASCII.GetString(buffer));

            } while (!sb.ToString().Contains("\r\n\r\n"));


            return sb.ToString();
        }
    }
}
