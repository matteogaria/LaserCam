using System;
using System.Collections.Generic;
using System.Linq;
using Geometry.Entities;
using Geometry.MathUtils;

namespace Geometry
{
    public class ShapeBuilder
    {
        public static IEnumerable<Line> PolygonByLength(int numSides, double length, string layer)
        {
            double radius = length / (2 * Math.Sin(Misc.ToRad(180 / numSides)));
            return PolygonByRadius(numSides, radius, false, layer);
        }

        public static IEnumerable<Line> PolygonByRadius(int numSides, double radius, bool centered, string layer)
        {
            double rotation = numSides switch
            {
                6 or 10 => 0,
                _ => Misc.ToRad(numSides % 2 == 0 ? (180 / numSides) : 90)
            };

            List<PointXY> points = new();
            for (int i = 0; i < numSides + 1; i++)
            {
                double angle = rotation + 2 * Math.PI * i / numSides;
                double x = radius * Math.Cos(angle);
                double y = radius * Math.Sin(angle);
                points.Add(new PointXY(x, y));
            }

            if (centered)
                return Build(points.ToArray());

            double xMin = points.Select(p => p.X).Min();
            double yMin = points.Select(p => p.Y).Min();
            IEnumerable<PointXY>  offsetPoints = points.Select(p => p.Offset(-xMin, -yMin));

            return Build(offsetPoints.ToArray());

            IEnumerable<Line> Build(PointXY[] points)
            {
                for(int i = 1; i < points.Length; i++)
                {
                    yield return new Line(points[i - 1], points[i], layer);
                }
               // yield return new Line(points[^1], points[0]);
            }
        }
    }
}
