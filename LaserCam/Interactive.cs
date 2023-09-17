using System;
using System.IO;
using System.Linq;
using Geometry;
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

            return Select<bool>("Choose an operation",
                ("load Dxf", RunFileInteractive),
                ("generate a shape", GenerateShape));
        }

        public bool GenerateShape()
        {
            int sides = Prompt.Input<int>("Number of sides", 3);
            double len = Prompt.Input<double>("Length", 10.0);

            CamSettingsModel choosenSettings = Prompt.Select("Please choose the desired profile", configuration.CamSettings, defaultValue: configuration.CamSettings[0], textSelector: s => s.Name);

            string layerName = Prompt.Select("Please choose the layer name from the profile", choosenSettings.Parameters.Select(p => p.Layer));
            bool optimizer = Prompt.Confirm("Disable travel path optimizer?", false);
            string outputFile = Select("Choose where to save the gcode",
                ("configured output", () => PrepareDefaultOutput("polygon")),
                ("use dialog", () => WindowsFileDialog.ShowSaveFileDialog("Save as...", $"Gcode (*.{configuration.OutputExtension})", $"*.{configuration.OutputExtension}")));

            var polygon = ShapeBuilder.PolygonByLength(sides, len, layerName);
            string error = null;
            int start = Environment.TickCount;
            bool ok = !string.IsNullOrEmpty(outputFile) && CAM.Run(choosenSettings, polygon, outputFile, optimizer, out error);
            int elapsed = Environment.TickCount - start;

            if (ok)
                Console.WriteLine($"Conversion completed, elapsed time: {elapsed}ms\r\nGenerated toolpath saved in {outputFile}");
            else
                Console.WriteLine($"ERROR: {error ?? "No output file specified"}");

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

        public bool RunFileInteractive()
        {
            if (!OpenFile(out string inputFile))
                return false;

            string inputFileClean = Path.GetFileNameWithoutExtension(inputFile);
            string inputPath = Path.GetDirectoryName(inputFile);

            Console.WriteLine($"File selected: {inputFile}");
            CamSettingsModel choosenSettings = Prompt.Select("Please choose the desired profile", configuration.CamSettings, defaultValue: configuration.CamSettings[0], textSelector: s => s.Name);
            bool optimizer = Prompt.Confirm("Disable travel path optimizer?", false);

            string outputFile = Select("Choose where to save the gcode",
                ("configured output", () => PrepareDefaultOutput(inputFileClean)),

                ("same as input", () => Path.Combine(inputPath, $"{inputFileClean}.{configuration.OutputExtension}")),

                ("use dialog", () => WindowsFileDialog.ShowSaveFileDialog("Save as...", $"Gcode (*.{configuration.OutputExtension})", $"*.{configuration.OutputExtension}")));

            int start = Environment.TickCount;
            bool ok = CAM.Run(choosenSettings, inputFile, outputFile, optimizer, out string error);
            int elapsed = Environment.TickCount - start;

            if (ok)
                Console.WriteLine($"Conversion completed, elapsed time: {elapsed}ms\r\nGenerated toolpath saved in {outputFile}");
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
