using System;
using System.Collections.Generic;

using Geometry.Entities;
using Geometry.MathUtils;

namespace Geometry
{
    public class ShapeBuilder
    {
        public static IEnumerable<GeometryObject> PolygonByLength(int numSides, double length)
        {
            double radius = length / (2 * Math.Sin(Misc.ToRad(180 / numSides)));
            return PolygonByRadius(numSides, radius);
        }

        public static IEnumerable<GeometryObject> PolygonByRadius(int numSides, double radius)
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

        }
    }
}
