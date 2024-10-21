// Program.cs
using System;
using System.Windows.Forms;

namespace InboxMonitoringApp
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                // Initialize application configuration
                ApplicationConfiguration.Initialize();
                Application.Run(new Form1());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Application failed to start: {ex.Message}", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Optionally log the error using the logging framework
                // Logger.Fatal(ex, "Application failed to start.");
            }
        }
    }
}
