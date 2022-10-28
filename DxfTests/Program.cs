//Copyright (c) 2020 Matteo Garia - https://github.com/matteogaria

// This program is free software; you can redistribute it and/or modify  it under the terms of the GPLv3 General Public License as published by  the Free Software Foundation; either version 3 of the License, or (at  your option) any later version.
// This program is distributed in the hope that it will be useful, but  WITHOUT ANY WARRANTY; without even the implied warranty of  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GPLv3  General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program.  If not, see <https://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;

using netDxf;

using Geometry.Entities;
using DxfTools;
using netDxf.Objects;
using System.Linq;
using Geometry.MathUtils;
using Geometry;

namespace DxfTests
{
    class Program
    {
        static double maxLen = 20;
        static double minLen = 4;
        static double shift = 0.5;
        static void Main(string[] args)
        {
            double n = 5;
            double r = 6;

            double rotation = n switch
            {
                6 or 10 => 0,
                _ => Misc.ToRad(n % 2 == 0 ? (180 / n) : 90)
            };

            List<double> dataX = new();
            List<double> dataY = new();
            for (int i = 0; i < n + 1; i++)
            {
                double angle = rotation + 2 * Math.PI * i / n;
                double x = r * Math.Cos(angle);
                double y = r * Math.Sin(angle);
                dataX.Add(x);
                dataY.Add(y);
            }

            double xmin = Math.Abs(dataX.Min());
            double ymin = Math.Abs(dataY.Min());

            dataX = dataX.Select(x => x + xmin).ToList();
            dataY = dataY.Select(y => y + ymin).ToList();

            var plot = new ScottPlot.Plot(800, 800);
            plot.AddScatter(dataX.ToArray(), dataY.ToArray());
            plot.AxisScaleLock(true, ScottPlot.EqualScaleMode.ZoomOut);

            plot.SaveFig("test.png");

            //Console.ReadLine();
            Environment.Exit(0);


            var doc = DxfDocument.Load("circle.dxf");
            var result = new DxfDocument();

            // conversion of circles
            //List<Circle> documentCircles = new();
            //foreach (netDxf.Entities.Circle dxfCircle in doc.Circles)
            //{
            //    Circle circle = new Circle(dxfCircle.Center.ToPoint(), dxfCircle.Radius);
            //    documentCircles.Add(circle);
            //}

            // exploding all lwpolylines to simple lines and arcs
            DxfTools.Extensions.ExplodeLwPoly(doc);

            // conversion of all lines in primitive line geometry
            List<Line> documentLines = new();
            //foreach (netDxf.Entities.Line dxfLine in doc.Lines)
            //{
            //    Line line = new Line(dxfLine.StartPoint.ToPoint(), dxfLine.EndPoint.ToPoint());

            //    if (line.Length > maxLen)
            //    {
            //        documentLines.AddRange(line.Split(maxLen));
            //    }
            //    else
            //    {
            //        documentLines.Add(line);
            //    }
            //}

            // recreating the dxf lines with shortening at edges
            List<netDxf.Entities.Line> dxfLines = new List<netDxf.Entities.Line>();
            foreach (Line line in documentLines)
            {

                if (line.Length < minLen)
                {
                    dxfLines.Add(new netDxf.Entities.Line(line.StartPoint.ToVector(), line.EndPoint.ToVector()));
                    continue;
                }

                PointXY newStartPoint;
                PointXY newEndPoint;
                if (line.StartPoint.X <= line.EndPoint.X)
                {
                    newStartPoint = line.StartPoint.Move(line.AngleRad, shift);
                    newEndPoint = line.EndPoint.Move(line.AngleRad, -shift);
                }
                else
                {
                    newStartPoint = line.StartPoint.Move(line.AngleRad, -shift);
                    newEndPoint = line.EndPoint.Move(line.AngleRad, shift);
                }

                dxfLines.Add(new netDxf.Entities.Line(newStartPoint.ToVector(), newEndPoint.ToVector()));
            }

            // replacing circles with arcs
            //List<netDxf.Entities.Arc> dxfArcs = new();
            //foreach (Circle circle in documentCircles)
            //{
            //    var arcs = circle.SplitToArcs(40);
            //    foreach(Arc arc in arcs)
            //    {
            //        var dxfArc = new netDxf.Entities.Arc(arc.Center.ToVector(), arc.Radius, arc.StartAngle, arc.EndAngle);
            //        dxfArcs.Add(dxfArc);
            //    }
            //}

            //adding lines to doc
            result.AddOrClone(dxfLines);

            //adding arcs to doc
            //result.AddOrClone(dxfArcs);

            //copy of other objects from original doc
            result.AddOrClone(doc.Arcs);
            result.AddOrClone(doc.Circles);

            result.Save("result.dxf");
            Console.WriteLine("Operazione completata!");
            Console.ReadKey();

        }
    }
}

