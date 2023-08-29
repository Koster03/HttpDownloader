namespace HttpFileDownloader.Win
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += ApplicationThreadException;
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }

        private static void ApplicationThreadException(object sender, ThreadExceptionEventArgs e)
        {
            string message = $"Sorry, something went wrong.\r\n\r\n{e.Exception.Message}";
            MessageBox.Show(message, @"Unexpected error");
        }
    }
}