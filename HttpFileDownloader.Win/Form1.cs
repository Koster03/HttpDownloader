using HttpFileDownloader.Core;
using System.Collections.Concurrent;

namespace HttpFileDownloader.Win
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private ConcurrentStack<EventInfo> regionsQueue = new ConcurrentStack<EventInfo>();

        private bool isDownloaded = false;
        private bool getException = false;

        // Иногда может зависать (не отвечает окно), особенно при первом запуске:), но все равно в итоге и отлагает и скачает
        // 
        // Samples Urls: https://www.sample-videos.com/img/Sample-jpg-image-15mb.jpeg
        //               https://www.sample-videos.com/gif/2.gif
        //               https://www.sample-videos.com/doc/Sample-doc-file-5000kb.doc
        //               https://www.sample-videos.com/zip/50mb.zip
        private async void button1_Click_1(object sender, EventArgs e)
        {
            this.Text = "Downloading...";
            labelStatus.Visible = false;
            button1.Enabled = false;
            mainProgressPanel.Visible = false;

            var url = this.textBox1.Text;

            await Task.Run(() => DownloadAsync(url));


            while (!isDownloaded)
            {
                if (getException)
                {
                    break;
                }

                if (regionsQueue.TryPop(out var info))
                {
                    regionsQueue.Clear();
                    await Task.Run(() => Tesr(info));
                    this.isDownloaded = info.isDownloaded;
                }
            }

            isDownloaded = false;

            ClearCoontrols();

            mainProgressPanel.BackColor = Color.FromArgb(0, 182, 79);
            mainProgressPanel.Visible = true;
            button1.Enabled = true;
            this.Text = "Ready for download";

            if (getException)
            {
                getException = false;
                mainProgressPanel.BackColor = Color.FromArgb(193, 0, 2);
                return;
            }

            labelStatus.Visible = true;
        }

        private void DownloadAsync(string url)
        {
            var downloader = new HTTTPDownloader(new FirstStrategy(10, 1048576));
            downloader.ChangeRegions += Downloader_ChangeRegions;

            try
            {
                downloader.Download(url);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                this.getException = true;
                return;
            }

        }

        private void ClearCoontrols()
        {
            foreach (var item in Controls)
            {
                if (item is Panel)
                {
                    var itm = (Panel)item;
                    if (itm.Name == "mainProgressPanel")
                    {
                        continue;
                    }
                    Controls.Remove(itm);
                }
            }
        }

        private void Tesr(EventInfo info)
        {
            ClearCoontrols();
            double correcting = info.size / (double)mainProgressPanel.Width;

            foreach (var region in info.regions.ToList())
            {
                switch (region.State)
                {
                    case RegionState.Free:
                        {
                            CreatePanel(correcting, region.start, region.end, Color.FromArgb(11, 143, 155));
                            break;
                        }
                    case RegionState.InProcess:
                        {
                            CreatePanel(correcting, region.start, region.end, Color.FromArgb(255, 126, 123));
                            break;
                        }
                    case RegionState.Downloaded:
                        {
                            CreatePanel(correcting, region.start, region.end, Color.FromArgb(0, 182, 79));
                            break;
                        }
                    case RegionState.Planned:
                        {
                            CreatePanel(correcting, region.start, region.end, Color.FromArgb(255, 128, 15));
                            break;
                        }
                }
            }
        }

        private void Downloader_ChangeRegions(object? sender, EventInfo info)
        {
            regionsQueue.Push(info);
        }

        private void CreatePanel(double correcting, long start, long end, Color color)
        {
            var p = new Panel();
            p.BackColor = color;
            p.Location = new Point(12 + (int)Math.Round(start / correcting, MidpointRounding.AwayFromZero), mainProgressPanel.Location.Y);
            p.Size = new Size((int)Math.Round((end - start + 1) / correcting, MidpointRounding.AwayFromZero), mainProgressPanel.Height);

            Controls.Add(p);
        }
        
    }
}