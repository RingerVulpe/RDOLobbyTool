using FileTool;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RDOLobbyTool
{
    static class Program
    {
        private const string ConfigFile = "config.txt";

        [STAThread]
        static void Main()
        {
            string folder = GetOrPromptFolder();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm(folder));
        }

        private static string GetOrPromptFolder()
        {
            if (File.Exists(ConfigFile))
                return File.ReadAllText(ConfigFile)!;

            // allocate a console so the user can type
            if (!AllocConsole())
                throw new Exception("Failed to open console");

            Console.Write("Enter target folder path: ");
            string? path = Console.ReadLine()?.Trim();
            // optional: validate Directory.Exists(path)
            File.WriteAllText(ConfigFile, path!);

            // free it so no console pops up later
            FreeConsole();

            return path!;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeConsole();
    }
}
