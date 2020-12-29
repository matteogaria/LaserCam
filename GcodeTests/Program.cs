//Copyright (c) 2020 Matteo Garia - https://github.com/matteogaria

// This program is free software; you can redistribute it and/or modify  it under the terms of the GPLv3 General Public License as published by  the Free Software Foundation; either version 3 of the License, or (at  your option) any later version.
// This program is distributed in the hope that it will be useful, but  WITHOUT ANY WARRANTY; without even the implied warranty of  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GPLv3  General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Globalization;
using System.Text;

using GCode.Codes;

namespace GcodeTests
{
    class Program
    {
        static void Main(string[] args)
        {

            StringBuilder sb;
            int tstart;
            int elapsed;

            sb = new();

            tstart = Environment.TickCount;
            for (int i = 0; i < 1000000; i++)
            {
                RapidMove rapidMove = new RapidMove(200, 200);
                rapidMove.Build(sb, 3);
                rapidMove = null;
            }
            elapsed = Environment.TickCount - tstart;

            Console.WriteLine($"gcode object mode: {elapsed} ms");

            sb = new();

            tstart = Environment.TickCount;
            for (int k = 0; k < 1000000; k++)
            {
             //   StaticGcode.RapidMove(sb, 200, 200);
            }
            elapsed = Environment.TickCount - tstart;

            Console.WriteLine($"static method mode: {elapsed} ms");

           

            sb = new();

            double coord = 200;
            tstart = Environment.TickCount;
            for (int i = 0; i < 1000000; i++)
            {
  
                sb.Append("G0");
                sb.Append("X");
                sb.Append(coord.ToString("0.000", CultureInfo.InvariantCulture));
                sb.Append("Y");
                sb.Append(coord.ToString("0.000", CultureInfo.InvariantCulture));
            }
            elapsed = Environment.TickCount - tstart;

            Console.WriteLine($"stupid mode: {elapsed} ms");

            Console.ReadKey();
        }
    }
}
