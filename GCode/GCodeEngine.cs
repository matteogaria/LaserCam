//Copyright (c) 2020 Matteo Garia - https://github.com/matteogaria

// This program is free software; you can redistribute it and/or modify  it under the terms of the GPLv3 General Public License as published by  the Free Software Foundation; either version 3 of the License, or (at  your option) any later version.
// This program is distributed in the hope that it will be useful, but  WITHOUT ANY WARRANTY; without even the implied warranty of  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GPLv3  General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Geometry.Entities;

using GCode.Codes;
using GCode.Models;

namespace GCode
{
    public class GCodeEngine
    {
        public GCodeEngine(GCodeEngineSettings settings)
        {
            layerSettings = settings.LayerSettings;
            decimalPlaces = settings.DecimalPlaces;

            currentPosition = new PointXY(0, 0);
            currentFeedRate = 0;
        }

        private readonly List<GCodeLayerSettings> layerSettings;
        private readonly int decimalPlaces;
        private double totalRapidMoveDistance = 0;

        private List<GCodeObject> generatedGCode = new();

        public void Run(IEnumerable<Shape> shapes, StringBuilder sb)
        {
            foreach (Shape s in shapes)
                BuildShape(s, sb);

            BuildLaser(LaserMode.Off, 0);

            foreach (GCodeObject gcode in generatedGCode)
            {
                gcode.Build(sb, decimalPlaces);
                sb.AppendLine();
            }

            Console.WriteLine($"Rapid move distance: {totalRapidMoveDistance}");
        }

        private void BuildShape(Shape shape, StringBuilder sb)
        {
            GCodeLayerSettings settings = LoadCurrentShapeSettings(shape);
            if (settings == null)
            {
                Console.WriteLine($"Shape skipped: no settings for layer {shape.First().RefName}");
                return;
            }

            bool reverse;
            if (PointXY.IsSamePoint(shape.StartPoint, currentPosition, settings.PointTolerance))
                reverse = false;
            else if (PointXY.IsSamePoint(shape.EndPoint, currentPosition, settings.PointTolerance))
                reverse = true;
            else
            {
                //TODO: replace StartPoint with nearest point
                BuildRapidMove(shape.StartPoint);
                reverse = false;
            }

            BuildLaser(settings.Mode, settings.Power);

            for (int i = 0; i < settings.Passes; i++)
            {
                BuildSinglePass(shape, settings, reverse, sb);
                reverse = !reverse;
            }
        }

        private GCodeLayerSettings LoadCurrentShapeSettings(Shape shape)
        {
            //TODO: create into shape object a proper settings property
            var settings = layerSettings.Where(s => s.RefName == shape.First().RefName).SingleOrDefault();
            return settings;
        }

        private void BuildSinglePass(Shape s, GCodeLayerSettings settings, bool reverse, StringBuilder sb)
        {
            Shape input = s.Clone();

            while (input.Count > 0)
            {
                GeometryObject geo = reverse ? input.Last() : input.First();
                input.Remove(geo);
                if (geo is Line l)
                    BuildLine(l, reverse, settings, sb);
                else if (geo is Arc a)
                    BuildArc(a, reverse, settings, sb);
                else if (geo is Circle c)
                    BuildCircle(c, settings, sb);
            }
        }

        private void BuildLine(Line l, bool reverse, GCodeLayerSettings settings, StringBuilder sb)
        {
            BuildLinearMove(reverse ? l.StartPoint : l.EndPoint, settings.FeedRate);
        }

        private void BuildCircle(Circle circle, GCodeLayerSettings settings, StringBuilder sb)
        {
            //TODO: implementation of check if a point is on the circumference
            //to optimize travels
            PointXY startPoint = PointXY.MovePoint(circle.Center, 0, circle.Radius);

            if (!PointXY.IsSamePoint(currentPosition, startPoint, settings.PointTolerance))
                BuildRapidMove(startPoint);

            BuildLaser(settings.Mode, settings.Power);

            PointXY offset = new PointXY(circle.Center.X - startPoint.X, circle.Center.Y - startPoint.Y);
            BuildCWArcMove(startPoint, offset, settings.FeedRate);

        }

        private void BuildArc(Arc arc, bool reverse, GCodeLayerSettings settings, StringBuilder sb)
        {
            BuildLaser(settings.Mode, settings.Power);

            if (reverse)
            {

                PointXY offset = new PointXY(arc.Center.X - arc.EndPoint.X, arc.Center.Y - arc.EndPoint.Y);

                if (arc.IsCW)
                    BuildCCWArcMove(arc.StartPoint, offset, settings.FeedRate);
                else
                    BuildCWArcMove(arc.StartPoint, offset, settings.FeedRate);
            }
            else
            {
                PointXY offset = new PointXY(arc.Center.X - arc.StartPoint.X, arc.Center.Y - arc.StartPoint.Y);

                if (arc.IsCW)
                    BuildCWArcMove(arc.EndPoint, offset, settings.FeedRate);
                else
                    BuildCCWArcMove(arc.EndPoint, offset, settings.FeedRate);
            }
        }

        private void BuildLaser(LaserMode mode, int power)
        {
            if (currentMode == mode && currentPower == power)
                return;

            GCodeObject command = mode switch
            {
                LaserMode.Fixed => new LaserOnFixed(power),
                LaserMode.Dynamic => new LaserOnDynamic(power),
                LaserMode.Off or _ => new LaserOff()
            };

            currentMode = mode;
            currentPower = power;
            generatedGCode.Add(command);
        }

        private void BuildLinearMove(PointXY destination, int feedRate)
        {
            LinearMove move;
            if (feedRate != currentFeedRate)
            {
                currentFeedRate = feedRate;
                move = new LinearMove(destination.X, destination.Y, feedRate);
            }
            else
            {
                move = new LinearMove(destination.X, destination.Y);
            }

            currentPosition = destination;
            generatedGCode.Add(move);
        }

        private void BuildRapidMove(PointXY destination)
        {
            totalRapidMoveDistance += PointXY.Distance(destination, currentPosition);

            RapidMove move = new RapidMove(destination.X, destination.Y);
            currentPosition = destination;
            generatedGCode.Add(move);

        }

        private void BuildCWArcMove(PointXY destination, PointXY offset, int feedRate)
        {
            CWArc move;
            if (feedRate != currentFeedRate)
            {
                currentFeedRate = feedRate;
                move = new CWArc(destination.X, destination.Y, offset.X, offset.Y, feedRate);
            }
            else
            {
                move = new CWArc(destination.X, destination.Y, offset.X, offset.Y);
            }

            currentPosition = destination;
            generatedGCode.Add(move);
        }

        private void BuildCCWArcMove(PointXY destination, PointXY offset, int feedRate)
        {
            CCWArc move;
            if (feedRate != currentFeedRate)
            {
                currentFeedRate = feedRate;
                move = new CCWArc(destination.X, destination.Y, offset.X, offset.Y, feedRate);
            }
            else
            {
                move = new CCWArc(destination.X, destination.Y, offset.X, offset.Y);
            }

            currentPosition = destination;
            generatedGCode.Add(move);

        }

        private PointXY currentPosition;
        private int currentFeedRate;
        private LaserMode currentMode;
        private int currentPower;
    }
}
