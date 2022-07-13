using System;
using System.IO;
using System.Linq;

using Sharprompt;

namespace LaserCam
{
    public class Interactive
    {
        private LaserCamConfiguration configuration;
        private readonly string settingsFilename;

        public Interactive(string settingsFilename)
        {
            this.settingsFilename = settingsFilename;
            string logo =
                " ______________________________________________________________\r\n" +
                "        _______ _______ _______  ______ _______ _______ _______\r\n" +
                " |      |_____| |______ |______ |_____/ |       |_____| |  |  |\r\n" +
                " |_____ |     | ______| |______ |    \\_ |_____  |     | |  |  |\r\n" +
                " ______________________________________________________________";

            Console.WriteLine(logo);


        }

        public bool Run()
        {
            configuration = LaserCamConfiguration.Load(settingsFilename);
            if (configuration == null || configuration.CamSettings.Count == 0)
            {
                Console.WriteLine("ERROR: Configuration not found or broken");
                return false;
            }

            if (!OpenFile(out string inputFile))
            {
                Console.WriteLine("ERROR: No file given");
                return false;
            }
            string inputFileClean = Path.GetFileNameWithoutExtension(inputFile);
            string inputPath = Path.GetDirectoryName(inputFile);

            Console.WriteLine($"File selected: {inputFile}");
            CamSettingsModel choosenSettings = Prompt.Select("Please choose the desired profile", configuration.CamSettings, defaultValue: configuration.CamSettings[0], textSelector: s => s.Name);
            bool optimizer = Prompt.Confirm("Disable travel path optimizer?", false);


            string outputFile = Select("Choose where to save the gcode",
                ("configured output", () => PrepareDefaultOutput(inputFileClean)),

                ("same as input", () => Path.Combine(inputPath, $"{inputFileClean}.{configuration.OutputExtension}")),

                ("use dialog", () => WindowsFileDialog.ShowSaveFileDialog("Save as...", $"Gcode (*.{configuration.OutputExtension})", $"*.{configuration.OutputExtension}")));

            bool ok = CAM.Run(choosenSettings, inputFile, outputFile, optimizer, out string error);

            if (ok)
                Console.WriteLine($"Generated toolpath saved in {outputFile}");
            else
                Console.WriteLine($"ERROR: {error}");

            return ok;

            string PrepareDefaultOutput(string name)
            {
                string path = Path.HasExtension(configuration.DefaultOutput) ?
                configuration.DefaultOutput :
                Path.Combine(configuration.DefaultOutput, $"{name}.{configuration.OutputExtension}");

                string dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                return path;
            }
        }

        private bool OpenFile(out string path)
        {
            path = String.Empty;
            bool run = true;
            while (run)
            {
                string inputFile = WindowsFileDialog.ShowOpenFileDialog("Select drawing", "DXF Vector drawing (*.dxf)", "*.dxf");
                if (string.IsNullOrEmpty(inputFile))
                    run = !Prompt.Confirm("No file selected, do you want to exit?", false);
                else
                {
                    path = inputFile;
                    break;
                }
            }

            return run;
        }


        static T Select<T>(string prompt, params (string, Func<T>)[] actions)
        {
            string selected = Prompt.Select(prompt, actions.Select(a => a.Item1), defaultValue: actions[0].Item1);
            Func<T> toDo = actions.Where(a => a.Item1.Equals(selected)).FirstOrDefault().Item2;
            return toDo.Invoke();
        }

    }
}
