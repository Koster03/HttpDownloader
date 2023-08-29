using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpFileDownloader.Core
{
    public class HTTTPDownloader
    {
        private readonly IDownloadStrategy downloadStrategy;

        private ParsedURL url;

        public string filePath;

        DownloadMap dm;

        private HTTPHelper HttpHelper = new HTTPHelper();

        public HTTTPDownloader(IDownloadStrategy downloadStrategy)
        {
            this.downloadStrategy = downloadStrategy;
        }

        public void Download(string url)
        {
            this.url = HTTP.ParseURL(url);

            string fileHeader = this.GetFileHeader();

            var fileSize = long.Parse(HTTP.GetHeadInfo("Content-Length", fileHeader));

            this.dm = new DownloadMap(fileSize, _ => this.ChangeRegions(this, _));

            filePath = FileWriter.CreateFile(this.url.file, fileSize, fileHeader);

            while (!dm.IsDownloaded)
            {
                this.downloadStrategy.DivideBLocks(this.dm);

                if (dm.PlannedCount() > 0)
                {
                    var plannedRegions = dm.GetRegions().Where(reg => reg.State == RegionState.Planned).ToArray();

                    foreach (var plannedRegion in plannedRegions)
                    {
                        plannedRegion.State = RegionState.InProcess;
                        Task.Run(() => DownloadRegion(plannedRegion, filePath));
                    }
                }
            }
        }

        public event EventHandler<EventInfo> ChangeRegions = delegate { };

        private void DownloadRegion(Region region, string path)
        {
            var req = HTTP.CreateHTTPRequest("GET", this.url,
               new Dictionary<string, string> { { "Accept", "*/*" }, { "User-Agent", "CSharpTests" },
                   { "Range", $"bytes={region.start}-{region.end}" }, { "X-Request-ID", $"{region.start}" } });


            using (var socket = HttpHelper.ConnectToURL(this.url))
            {
                HttpHelper.GetBody(req, socket, region, this.dm, this.filePath);
            }

            //FileWriter.Write(data, (int)region.start, path);

            region.State = RegionState.Downloaded;

            dm.ConcatRegions(region);
        }

        private string GetFileHeader()
        {
            var req = HTTP.CreateHTTPRequest("HEAD", this.url,
               new Dictionary<string, string> { { "Accept", "*/*" }, { "User-Agent", "CSharpTests" } });

            string header;

            using (var socket = HttpHelper.ConnectToURL(this.url))
            {
                header = this.HttpHelper.GetHead(req, socket);
            }

            return header;
        }
    }
}
