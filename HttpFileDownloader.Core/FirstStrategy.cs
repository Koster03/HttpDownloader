using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpFileDownloader.Core
{
    public class FirstStrategy : IDownloadStrategy
    {
        public int maxBlocksCount;
        public long maxBlockSize;

        public FirstStrategy(int maxBlocksCount, int maxBlockSize)
        {
            this.maxBlocksCount = maxBlocksCount;
            this.maxBlockSize = maxBlockSize;
        }

        public void DivideBLocks(DownloadMap dm)
        {
            while (dm.PlannedCount() + dm.InProcessCount() < maxBlocksCount)
            {
                if (dm.FreeCount() == 0)
                {
                    break;
                }
                var freeBlock = dm.GetRegions().First(i => i.State == RegionState.Free);

                if (freeBlock.Size > 2 * maxBlockSize)
                {
                    dm.MarkRegion(freeBlock.start, freeBlock.Size / 2, RegionState.Free);
                }
                else if (freeBlock.Size > maxBlockSize)
                {
                    dm.MarkRegion(freeBlock.start, maxBlockSize, RegionState.Planned);
                }
                else
                {
                    dm.MarkRegion(freeBlock.start, freeBlock.Size, RegionState.Planned);
                }
            }
        }
    }
}
