using Sharprompt;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LaserCam
{
    public class Interactive
    {
        private readonly List<CamSettings> settings;
        public Interactive(string settingsFilename)
        {
            settings = CamSettings.Load(settingsFilename);

        }
        public void Run()
        {
            if (!OpenFile(out string inputFile))
                return;

            CamSettings choosenSettings = Prompt.Select("Choose the desired profile", settings, defaultValue: settings[0], textSelector: s => s.Name);

            string inputFileClean = Path.GetFileNameWithoutExtension(inputFile);
            string inputPath = Path.GetDirectoryName(inputFile);
            string outputFile = Select("Choose where to save the gcode",
                ("configured output", () => $"C:\\{inputFileClean}.gcode"),
                ("same as input", () => Path.Combine(inputPath, $"{inputFileClean}.gcode")),
                ("use dialog", () => WindowsFileDialog.ShowSaveFileDialog("Save as...", "Gcode File (*.gcode)", "*.gcode")));
            bool optimizer = Prompt.Confirm("Disable optimizer?", false);

            CAM.Run(choosenSettings, inputFile, outputFile, optimizer);
            Console.WriteLine("Bye bye...");
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
