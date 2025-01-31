﻿//Copyright (c) 2020 Matteo Garia - https://github.com/matteogaria

// This program is free software; you can redistribute it and/or modify it under the terms of the GPLv3 General Public License as published by  the Free Software Foundation; either version 3 of the License, or (at  your option) any later version.
// This program is distributed in the hope that it will be useful, but  WITHOUT ANY WARRANTY; without even the implied warranty of  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GPLv3  General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;


namespace LaserCam
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                RootCommand cmd = new RootCommand
                {
                    new Option<string>(new[]{ "--input" , "-i" }, "path to input dxf file").AsRequired(),
                    new Option<string>(new[]{ "--output" , "-o" }, "path to output gcode file"),
                    new Option<string>(new[]{ "--profile" , "-p" }, "profile").AsRequired(),
                    new Option("--no-optimizer", "disables travel path optimizer"),

                    new Command("--license", "Get license information").WithHandler(nameof(ShowLicenseInfo))
                }.WithHandler(nameof(RunCam));

                cmd.Invoke(args);
            }
            else
            {
                bool ok = new Interactive("profiles.json").Run();
            }

            Console.WriteLine("Bye bye...");
        }

        private static void RunCam(string input, string output, string profile, bool noOptimizer, IConsole console)
        {
            LaserCamConfiguration settings = LaserCamConfiguration.Load("profiles.json");
            CamSettingsModel selectedSettings = settings.CamSettings.Where(s => s.Name.ToLower() == profile.ToLower()).SingleOrDefault();
            if (selectedSettings == null)
                console.Error.Write($"No profile is found with name {profile}");
            else
            {
                bool ok = CAM.Run(selectedSettings, input, output, noOptimizer, out string error);
                console.Error.Write(error);
            }

            console.Out.Write("Conversion done!\r\n");
        }

        private static void ShowLicenseInfo(IConsole console)
        {
            string license =
                "LaserCam.exe - A 2D laser engraving/cutting oriented CAM software\r\n" +
                "Copyright (c) 2020 Matteo Garia - https://github.com/matteogaria\r\n\r\n" +
                "This program is free software; you can redistribute it and/or modify  it under the terms of the GPLv3 General Public License as published by  the Free Software Foundation; either version 3 of the License, or (at  your option) any later version.\r\nThis program is distributed in the hope that it will be useful, but  WITHOUT ANY WARRANTY; without even the implied warranty of  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GPLv3  General Public License for more details.\r\nYou should have received a copy of the GNU General Public License along with this program.  If not, see https://www.gnu.org/licenses/.";

            console.Out.Write(license);
        }
    }

    public static class Ext
    {
        public static T WithHandler<T>(this T command, string name) where T : Command
        {
            var flags = BindingFlags.NonPublic | BindingFlags.Static;
            var method = typeof(Program).GetMethod(name, flags);

            var handler = CommandHandler.Create(method!);
            command.Handler = handler;
            return command;
        }

        public static Option AsRequired(this Option option)
        {
            option.IsRequired = true;
            return option;
        }
    }
}
