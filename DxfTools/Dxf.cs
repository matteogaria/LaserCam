//Copyright (c) 2020 Matteo Garia - https://github.com/matteogaria

// This program is free software; you can redistribute it and/or modify  it under the terms of the GPLv3 General Public License as published by  the Free Software Foundation; either version 3 of the License, or (at  your option) any later version.
// This program is distributed in the hope that it will be useful, but  WITHOUT ANY WARRANTY; without even the implied warranty of  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GPLv3  General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;

using netDxf;
using netDxf.IO;

using Geometry.Entities;

namespace DxfTools
{
    public class Dxf
    {
        private DxfDocument doc;
        private readonly string path;

        public Dxf(string path)
        {
            this.path = path;
        }

        public bool Load(out string error)
        {
            try
            {
                error = string.Empty;
                doc = DxfDocument.Load(path);
                return true;
            }
            catch (DxfVersionNotSupportedException)
            {
                error = "dxf file version too old: only AutoCad2000 and higher are supported";
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            return false;

        }

        public IEnumerable<GeometryObject> Parse()
        {
            // exploding all lwpolylines to simple lines and arcs
            Extensions.ExplodeLwPoly(doc);

            // conversion of all lines in primitive line geometry
            List<GeometryObject> geometries = new();

            geometries.AddRange(LoadLines(doc.Entities.Lines));
            geometries.AddRange(LoadCircles(doc.Entities.Circles));
            geometries.AddRange(LoadArcs(doc.Entities.Arcs));

            return geometries;
        }

        private static IEnumerable<GeometryObject> LoadLines(IEnumerable<netDxf.Entities.Line> dxfLines)
        {
            List<GeometryObject> retVal = new();
            retVal.AddRange(dxfLines.Select(dxfLine => new Line(dxfLine.StartPoint.ToPoint(), dxfLine.EndPoint.ToPoint(), dxfLine.Layer.Name)));

            return retVal;
        }

        private static IEnumerable<GeometryObject> LoadCircles(IEnumerable<netDxf.Entities.Circle> circles)
        {
            List<GeometryObject> retVal = new();
            retVal.AddRange(circles.Select(dxfCircle => new Circle(dxfCircle.Center.ToPoint(), dxfCircle.Radius, dxfCircle.Layer.Name)));

            return retVal;
        }

        private static IEnumerable<GeometryObject> LoadArcs(IEnumerable<netDxf.Entities.Arc> arcs)
        {
            List<GeometryObject> retVal = new();
            // arcs in dxf documents are assumed CCW by definition
            retVal.AddRange(arcs.Select(dxfArc => new Arc(dxfArc.Center.ToPoint(), dxfArc.Radius, dxfArc.StartAngle, dxfArc.EndAngle, false, dxfArc.Layer.Name)));

            return retVal;
        }
    }
}
