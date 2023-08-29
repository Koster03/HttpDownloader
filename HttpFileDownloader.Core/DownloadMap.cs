using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HttpFileDownloader.Core
{
    public record EventInfo(long size, List<Region> regions, bool isDownloaded);

    public class DownloadMap : IDownloadMap
    {
        private Action<EventInfo> action;
        private long fileSize;
        public DownloadMap(long size, Action<EventInfo> action)
        {
            fileSize = size;
            this.action = action;
            regions.Add(new Region(0, size - 1, RegionState.Free));
        }

        private List<Region> regions = new();

        [MethodImpl(MethodImplOptions.Synchronized)]
        public int DowloadedCount()
        {
            return this.regions.Count(i => i.State == RegionState.Downloaded);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public int PlannedCount()
        {
            return this.regions.Count(i => i.State == RegionState.Planned);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public int InProcessCount()
        {
            return this.regions.Count(i => i.State == RegionState.InProcess);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public int FreeCount()
        {
            return this.regions.Count(i => i.State == RegionState.Free);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void MarkRegion(long start, long length, RegionState state)
        {
            var reg = this.regions.First(i => i.start == start);

            if (reg is null)
            {
                throw new Exception();
            }

            if (length == reg.Size)
            {
                reg.State = state;
                this.action(new EventInfo(this.fileSize, GetRegions(), this.IsDownloaded));
                return;
            }

            this.regions.Remove(reg);
            var newReg = new Region(start, start + length - 1, state);

            this.regions.Add(newReg);
            this.regions.Add(new Region(start + length, reg.end, reg.State));

            this.regions = this.regions.OrderBy(i => i.start).ToList();

            this.ConcatRegions(newReg);

            this.action(new EventInfo(this.fileSize, GetRegions(), this.IsDownloaded));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ConcatRegions(Region newReg)
        {
            var prevReg = this.regions.Find(i => i.end == newReg.start - 1);

            if (prevReg is null) return;

            if (prevReg.State == newReg.State && newReg.State == RegionState.Downloaded)
            {
                var start = prevReg.start;
                var end = newReg.end;

                this.regions.Remove(prevReg);
                this.regions.Remove(newReg);

                this.regions.Add(new Region(start, end, newReg.State));

                this.regions = this.regions.OrderBy(i => i.start).ToList();
            }

            this.action(new EventInfo(this.fileSize, GetRegions(), this.IsDownloaded));
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public List<Region> GetRegions()
        {
            return this.regions;
        }

        public bool IsDownloaded
        {
            get => (this.regions.Count == 1 && this.regions[0].State == RegionState.Downloaded)
                || (this.DowloadedCount() == this.regions.Count);
        }
    }

    public enum RegionState
    {
        Free,
        Downloaded,
        Planned,
        InProcess
    }

    public class Region
    {

        public long start;
        public long end;

        public long Size
        {
            get { return this.end + 1 - start; }
        }

        public Region(long offset, long end, RegionState state)
        {
            this.start = offset;
            this.end = end;
            this.State = state;
        }

        public RegionState State;
    }
}
