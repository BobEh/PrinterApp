using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using System.Linq;
using System.Security.Permissions;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

namespace PrinterApp
{
    public partial class PrinterApp : ServiceBase
    {
        private static Font printFont;
        private static StreamReader streamToPrint;
        private static string printer = "";
        public static void Main()
        {
            Run();
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        private static void Run()
        {
            string[] args = Environment.GetCommandLineArgs();

            // If a directory is not specified, exit program.
            if (args.Length < 2)
            {
                // Display the proper way to call the program.
                Console.WriteLine("Usage: PrinterApp.exe (directory) (printer)");
                return;
            }
            else if (args.Length == 3)
            {
                printer = args[2];
            }

            // Create a new FileSystemWatcher and set its properties.
            using (FileSystemWatcher watcher = new FileSystemWatcher())
            {
                watcher.Path = args[1];

                // Watch for changes in LastAccess and LastWrite times, and
                // the renaming of files or directories.
                watcher.NotifyFilter = NotifyFilters.LastAccess
                                     | NotifyFilters.LastWrite
                                     | NotifyFilters.FileName
                                     | NotifyFilters.DirectoryName;

                // Only watch ini files.
                watcher.Filter = "*.ini";

                // Add event handler.
                watcher.Created += SendToPrinter;

                // Begin watching.
                watcher.EnableRaisingEvents = true;

                // Wait for the user to quit the program.
                Console.WriteLine("Press 'q' 'Enter' to quit the program.");
                while (Console.Read() != 'q') ;
            }
        }

        // The PrintPage event is raised for each page to be printed.
        private static void pd_PrintPage(object sender, PrintPageEventArgs ev)
        {
            float linesPerPage = 0;
            float yPos = 0;
            int count = 0;
            float leftMargin = ev.MarginBounds.Left;
            float topMargin = ev.MarginBounds.Top;
            String line = null;

            // Calculate the number of lines per page.
            linesPerPage = ev.MarginBounds.Height /
               printFont.GetHeight(ev.Graphics);

            // Iterate over the file, printing each line.
            while (count < linesPerPage &&
               ((line = streamToPrint.ReadLine()) != null))
            {
                yPos = topMargin + (count * printFont.GetHeight(ev.Graphics));
                ev.Graphics.DrawString(line, printFont, Brushes.Black,
                   leftMargin, yPos, new StringFormat());
                count++;
            }

            // If more lines exist, print another page.
            if (line != null)
                ev.HasMorePages = true;
            else
                ev.HasMorePages = false;
        }

        private static void SendToPrinter(object sender, FileSystemEventArgs e)
        {
            try
            {
                streamToPrint = new StreamReader(e.FullPath);
                try
                {
                    printFont = new Font("Arial", 10);
                    PrintDocument pd = new PrintDocument();
                    //pd.PrintPage += new PrintPageEventHandler(pd_PrintPage);
                    // Print the document.
                    //specify the printer
                    if (printer != "")
                    {
                        pd.PrinterSettings.PrinterName = printer;
                        if (pd.PrinterSettings.IsValid)
                        {
                            pd.Print();
                        }
                        else
                        {
                            Console.WriteLine("Printer is invalid.");
                        }
                    }
                    else
                    {
                        pd.Print();
                    }
                }
                finally
                {
                    streamToPrint.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
