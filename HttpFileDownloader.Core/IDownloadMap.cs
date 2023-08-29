using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpFileDownloader.Core
{
    public interface IDownloadMap
    {
        void MarkRegion(long offset, long length, RegionState state);

        List<Region> GetRegions();

        bool IsDownloaded { get; }
    }
}
