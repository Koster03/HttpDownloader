using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HttpFileDownloader.Core
{
    public static class FileWriter
    {
        static ReaderWriterLockSlim locker = new ReaderWriterLockSlim();
        public static void Write(byte[] data, int start, string path)
        {
            locker.EnterWriteLock();
            try
            {
                using (var stream = new FileStream(path, FileMode.Open))
                {
                    stream.Seek(start, SeekOrigin.Begin);
                    stream.Write(data, 0, data.Length);
                }
            }
            finally
            {
                locker.ExitWriteLock();
            }
        }

        public static string CreateFile(string filename, long size, string fileHeader)
        {
            string userRoot = Environment.GetEnvironmentVariable("USERPROFILE");
            string downloadFolder = Path.Combine(userRoot, "Downloads");

            if (!Path.HasExtension(filename))
            {
                string fileType = HTTP.GetHeadInfo("Content-Type", fileHeader);

                filename = string.Join('.', fileType.Split('/'));
            }

            var path = Path.Combine(downloadFolder, filename);

            while (File.Exists(path))
            {
                var pth = Path.GetFileNameWithoutExtension(path);
                var correct = " (1)";

                if (pth.Length > 3)
                {
                    var s = pth.Substring(pth.Length - 3);

                    if (s[0] == '(' && char.IsDigit(s[1]) && s[2] == ')')
                    {
                        var num = int.Parse(s[1].ToString());
                        correct = $" ({++num})";
                        pth = pth.Remove(pth.Length - 4);
                    }

                }

                pth += correct;

                path = Path.Combine(downloadFolder, pth + Path.GetExtension(filename));
            }

            var file = File.Create(path);
            file.Close();

            File.WriteAllBytes(path, new byte[size]);

            return path;
        }
    }
}
