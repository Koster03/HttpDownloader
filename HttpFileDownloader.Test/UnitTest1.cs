using HttpFileDownloader.Core;
using NUnit.Framework;

namespace HttpFileDownloader.Test
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {

            var downloader = new HTTTPDownloader(new FirstStrategy(10, 1048576));

            downloader.Download("https://www.sample-videos.com/img/Sample-png-image-30mb.png");
        }
    }
}