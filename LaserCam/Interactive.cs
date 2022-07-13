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

        public bool Run(out string error)
        {
            error = string.Empty;
            configuration = LaserCamConfiguration.Load(settingsFilename);
            if (configuration == null || configuration.CamSettings.Count == 0)
            {
                error = "Configuration not found or broken";
                return false;
            }

            if (!OpenFile(out string inputFile))
            {
                error = "No file given";
                return false;
            }

            CamSettingsModel choosenSettings = Prompt.Select("Choose the desired profile", configuration.CamSettings, defaultValue: configuration.CamSettings[0], textSelector: s => s.Name);

            string inputFileClean = Path.GetFileNameWithoutExtension(inputFile);
            string inputPath = Path.GetDirectoryName(inputFile);
            string outputFile = Select("Choose where to save the gcode",
                ("configured output", () => GetDefaultOutputName(inputFileClean)),

                ("same as input", () => Path.Combine(inputPath, $"{inputFileClean}.{configuration.OutputExtension}")),

                ("use dialog", () => WindowsFileDialog.ShowSaveFileDialog("Save as...", $"Gcode (*.{configuration.OutputExtension})", $"*.{configuration.OutputExtension}")));

            bool optimizer = Prompt.Confirm("Disable optimizer?", false);

            return CAM.Run(choosenSettings, inputFile, outputFile, optimizer, out error);

            string GetDefaultOutputName(string name)
                => File.GetAttributes(configuration.DefaultOutput).HasFlag(FileAttributes.Directory) ?
                Path.Combine(configuration.DefaultOutput, $"{name}.{configuration.OutputExtension}") :
                configuration.DefaultOutput;
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
