using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using WitcherScriptMerger.Forms;
using WitcherScriptMerger.Inventory;
using WitcherScriptMerger.LoadOrder;

namespace WitcherScriptMerger
{
    static class Program
    {
        public static AppSettings Settings = new AppSettings();
        public static CustomLoadOrder LoadOrder = null;
        public static MergeInventory Inventory = null;
        public static MainForm MainForm;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (!Settings.HasConfigFile)
            {
                ShowLaunchFailure("Config file is missing.");
                return;
            }
            if (!Paths.ValidateDependencyPaths())
            {
                using (var dependencyForm = new DependencyForm())
                {
                    if (dependencyForm.ShowDialog() != DialogResult.OK)
                    {
                        ShowLaunchFailure("A dependency is missing.");
                        return;
                    }
                }
            }

            // Naive way to tell the program that its been executed through
            //  Vortex - this will have to be enhanced if we're planning
            //  on implementing some sort of automatic merging capability.
            if (args.Length == 1 && args[0] == "vortex")
            {
                Settings.Set("RanThroughVortex", true);
            }
            else
            {
                Settings.Set("RanThroughVortex", false);
            }

            MainForm = new MainForm();
            Application.Run(MainForm);
        }

        static void ShowLaunchFailure(string message)
        {
            MessageBox.Show(
                $"Launch failure: {message}",
                "Script Merger Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        public static bool TryOpenFile(string path)
        {
            if (!File.Exists(path))
            {
                MainForm.ShowMessage("Can't find file: " + path);
                return false;
            }

            if (path.EndsWithIgnoreCase(".exe"))  // EXEs need working dir to be specified
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = path,
                    WorkingDirectory = Path.GetDirectoryName(path)
                };
                Process.Start(startInfo);
            }
            else
                try { Process.Start(path); }
                catch (Exception) { }

            return true;
        }

        public static bool TryOpenFileLocation(string filePath)
        {
            return TryOpenDirectory(Path.GetDirectoryName(filePath));
        }

        public static bool TryOpenDirectory(string dirPath)
        {
            if (!Directory.Exists(dirPath))
            {
                MainForm.ShowMessage("Can't find directory: " + dirPath);
                return false;
            }
            Process.Start(dirPath);
            return true;
        }
    }
}