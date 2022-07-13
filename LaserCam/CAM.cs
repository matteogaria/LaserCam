//Copyright (c) 2020 Matteo Garia - https://github.com/matteogaria

// This program is free software; you can redistribute it and/or modify  it under the terms of the GPLv3 General Public License as published by  the Free Software Foundation; either version 3 of the License, or (at  your option) any later version.
// This program is distributed in the hope that it will be useful, but  WITHOUT ANY WARRANTY; without even the implied warranty of  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GPLv3  General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using DxfTools;
using GCode;
using Geometry.Entities;

namespace LaserCam
{
    public class CAM
    {
        public static bool Run(CamSettingsModel settings, string inputFile, string outputFile, bool disableOptimizer, out string error)
        {
            error = string.Empty;
            Dxf dxf = new Dxf(inputFile);

            if (!dxf.Load(out error))
                return false;

            IEnumerable<GeometryObject> geometries = dxf.Parse();
            IEnumerable<Shape> shapes = Shape.CreateShapes(geometries, settings.GetShapeSettings(), !disableOptimizer).ToList();
            GCodeEngine engine = new GCodeEngine(settings.GetGCodeSettings());

            StringBuilder sb = new();
            engine.Run(shapes, sb);

            File.WriteAllText(outputFile, sb.ToString());
            return true;
        }
    }
}
