//Copyright (c) 2020 Matteo Garia - https://github.com/matteogaria

// This program is free software; you can redistribute it and/or modify  it under the terms of the GPLv3 General Public License as published by  the Free Software Foundation; either version 3 of the License, or (at  your option) any later version.
// This program is distributed in the hope that it will be useful, but  WITHOUT ANY WARRANTY; without even the implied warranty of  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GPLv3  General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;

using Geometry.Entities;
//using NLog;

namespace GCode
{
    public class SectionSettings
    {
        public string LayerName { get; set; }
        public double PointTolerance { get; set; }
        public bool SplitGeometries { get; set; }
        public double SectionLength { get; set; }
        public double SectionTolerance { get; set; }
        public bool Ordered { get; set; }
    }

    public class Section : List<GeometryObject>
    {
        // private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public PointXY StartPoint { get => this.First().StartPoint; }
        public PointXY EndPoint { get => this.Last().EndPoint; }
        public double Perimeter { get => this.Select(g => g.Length).Sum(); }
        public double Tolerance { get; protected set; }
        public bool IsOrdered { get; protected set; } = false;
        public bool IsCircleShape { get => this.First().GetType() == typeof(Circle); }

        public Section Clone()
        {
            return new Section(Tolerance, this);
        }

        public static IEnumerable<Section> CreateShapes(IEnumerable<GeometryObject> geometries, IEnumerable<SectionSettings> settings, bool optimize = true)
        {
            var layers = geometries.GroupBy(geo => geo.RefName).Select(group => group.ToList()).ToList();
            List<Section> retVal = new List<Section>();

            foreach (var layer in layers)
            {
                string currentLayer = layer.First().RefName;
                var currentLayerSettings = settings.Where(s => s.LayerName == currentLayer).SingleOrDefault();

                if (currentLayerSettings == null)
                {
                    //logger.Info($"layer {currentLayer} will be skipped, no settings found");
                    continue;
                }

                var layerShapes = CreateSingleLayerShapes(layer, currentLayerSettings.PointTolerance, currentLayerSettings.Ordered).ToList();

                if (optimize)
                    Optimize(layerShapes, new PointXY(0, 0));

                if (currentLayerSettings.SplitGeometries)
                {
                    foreach (Section shape in layerShapes)
                    {
                        retVal.AddRange(
                            shape.Split(
                            currentLayerSettings.SectionLength,
                            currentLayerSettings.SectionTolerance,
                            currentLayerSettings.PointTolerance
                            ));
                    }

                }
                else
                    retVal.AddRange(layerShapes);
            }

            return retVal;
        }

        public void Order()
        {
            IsOrdered = true;
            if (Count == 1)
                return;

            for (int i = 0; i < Count; i++)
            {
                GeometryObject geo = this[i];
                GeometryObject prev = i == 0 ? null : this[i - 1];

                if (prev != null)
                {
                    //prev is ALWAYS correctly oriented due to first iteration calculus
                    //so there's only need to correct current geometry
                    if (PointXY.IsSamePoint(geo.EndPoint, prev.EndPoint, Tolerance))
                        geo.Reverse();
                }
                else
                {
                    //first iteration only
                    GeometryObject next = this[i + 1];
                    if (PointXY.IsSamePoint(geo.StartPoint, next.StartPoint, Tolerance) ||
                        PointXY.IsSamePoint(geo.StartPoint, next.EndPoint, Tolerance))
                        geo.Reverse();
                }
            }
        }

        public IEnumerable<Section> Split(double maxLength, double sectionTolerance, double pointTolerance)
        {
            List<Section> result = new();

            //circles are managed separately
            if (IsCircleShape)
            {
                result.AddRange(SplitCircle((Circle)this.First(), maxLength, pointTolerance));
                return result;
            }

            int sections = (int)Math.Ceiling(Perimeter / maxLength);
            double sectionLen = Math.Ceiling(Perimeter / sections);

            Section input = Clone();

            double remainingSectionLen = sectionLen;
            Section currentSection = new(Tolerance, null);
            while (input.Count > 0)
            {
                GeometryObject geometry = input.First();
                input.Remove(geometry);

                if (geometry.Length <= remainingSectionLen + sectionTolerance)
                {
                    currentSection.Add(geometry);
                    remainingSectionLen -= geometry.Length;
                }
                else
                {
                    IEnumerable<GeometryObject> geometrySections = SplitGeometry(geometry, remainingSectionLen, pointTolerance).Reverse();
                    foreach (GeometryObject obj in geometrySections)
                        input.Insert(0, obj);
                }

                if (remainingSectionLen - Tolerance <= 0 || input.Count == 0)
                {
                    result.Add(currentSection);
                    remainingSectionLen = sectionLen;
                    currentSection = new(Tolerance, null);
                }

            }

            return result;
        }

        public static void Optimize(List<Section> shapes, PointXY startPoint)
        {
            List<Section> input = new();
            input.AddRange(shapes);

            PointXY reference = startPoint;

            shapes.Clear();
            while (input.Count > 0)
            {

                var nearest = Nearest(input, reference, out PointXY nearestPoint).First();
                shapes.Add(nearest);
                input.Remove(nearest);

                if (nearestPoint == nearest.StartPoint)
                    reference = nearest.EndPoint;
                else
                    reference = nearest.StartPoint;

            }

        }

        public static IEnumerable<Section> Nearest(IEnumerable<Section> shapes, PointXY reference, out PointXY nearestPoint)
        {
            List<PointXY> points = shapes.SelectMany(s => new[] { s.StartPoint, s.EndPoint }).ToList();

            PointXY candidate = PointXY.InvalidPoint;
            double lastdistance = double.PositiveInfinity;
            foreach (PointXY point in points)
            {
                double distance = PointXY.Distance(reference, point);
                if (distance < lastdistance)
                {
                    candidate = point;
                    lastdistance = distance;
                }
            }

            var result = shapes.Where(s => s.StartPoint == candidate || s.EndPoint == candidate);
            nearestPoint = candidate;
            return result;
        }

        protected Section(double tolerance, IEnumerable<GeometryObject> geometries)
        {
            Tolerance = tolerance;
            if (geometries != null)
                AddRange(geometries);
        }

        private static IEnumerable<Section> CreateSingleLayerShapes(IEnumerable<GeometryObject> geometries, double tolerance, bool ordered)
        {
            // int skippedGeometries = geometries.Where(g => g.Length <= tolerance).Count();
            // if (skippedGeometries > 0)
            //     Console.WriteLine($"layer {geometries.First().RefName}: {skippedGeometries} geometries skipped");

            List<GeometryObject> input = geometries.Where(g => g.Length > tolerance && g.GetType() != typeof(Circle)).ToList();
            List<GeometryObject> circles = geometries.Where(g => g.Length > tolerance && g.GetType() == typeof(Circle)).ToList();

            List<Section> shapes = new List<Section>();
            // circles are closed objects by definition, so every single circles defines a shape
            shapes.AddRange(circles.Select(c => new Section(tolerance, new[] { c })));

            GeometryObject lastGeometry = null;
            Section s = null;
            bool toAdd = false;

            while (input.Count() > 0)
            {
                // first geometry in shape  -> always add
                if (lastGeometry == null)
                {
                    s = new Section(tolerance, null);
                    GeometryObject geometry = input.First();
                    s.Add(geometry);
                    lastGeometry = geometry;
                    input.Remove(geometry);

                }
                //other geometries
                else
                {
                    var geo = input.Where(i => GeometryObject.AreContinuous(i, lastGeometry, tolerance)).FirstOrDefault();
                    if (geo != null)
                    {
                        s.Add(geo);
                        lastGeometry = geo;
                        input.Remove(geo);
                    }
                    else
                    {
                        lastGeometry = null;
                        toAdd = true;
                    }
                }

                if (toAdd || input.Count == 0)
                {
                    toAdd = false;
                    if (ordered)
                        s.Order();

                    shapes.Add(s);
                }
            }

            return shapes;
        }

        private static IEnumerable<Section> SplitCircle(Circle circle, double maxLength, double pointTolerance)
        {
            List<Section> result = new();
            var splitted = circle.SplitToArcs(maxLength);

            if (splitted.Count() == 0)
            {
                result.Add(new Section(pointTolerance, new[] { circle }));
            }
            else
            {
                result.AddRange(splitted.Select(s => new Section(pointTolerance, new[] { s })));
            }

            return result;
        }

        private static IEnumerable<GeometryObject> SplitGeometry(GeometryObject geometry, double sectionlen, double pointTolerance)
        {
            if (geometry is Line line)
            {
                return line.SplitAt(sectionlen, pointTolerance);
            }
            else if (geometry is Arc arc)
                return arc.SplitAt(sectionlen);

            throw new ArgumentException($"SplitGeometry: can't split {geometry.GetType()} objects");
        }
    }
}
