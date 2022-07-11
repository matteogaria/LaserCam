using Sharprompt;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LaserCam
{
    public class Interactive
    {
        internal enum Options
        {
            [Display(Name = "Process a drawing")]
            ProcessDrawing,
            [Display(Name = "Create a new profile")]
            CreateProfile,
            [Display(Name = "Close LaserCam")]
            Exit
        }
        public void Run()
        {
            bool running = true;
            while (running)
                Select("Choose an action", new (Options, Action)[]
                    {
                    (Options.Exit, () => running = false),
                    (Options.ProcessDrawing, ProcessDrawing),
                    });

            Console.WriteLine("Bye bye...");
        }

        private void ProcessDrawing()
        {

        }

        static void Select<T>(string prompt, IEnumerable<(T, Action)> actions) where T : Enum
        {
            T selected = Prompt.Select<T>(prompt);
            Console.Title = selected.ToString();
            Action toDo = actions.Where(a => a.Item1.Equals(selected)).FirstOrDefault().Item2;
            toDo?.Invoke();
        }

    }
}
