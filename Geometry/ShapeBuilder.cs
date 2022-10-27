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
            double centerX;
            double centerY = radius / (2 * Math.Tan(Misc.ToRad(180 / numSides)));
            if (numSides % 2 == 0)
                centerX = centerY;
            else
                centerX = radius;

            //double rotation = Misc.ToRad(n % 2 == 0 ? 45 : 90);
            double rotation = numSides switch
            {
                6 or 10 => 0,
                _ => Misc.ToRad(numSides % 2 == 0 ? (180 / numSides) : 90)
            };

            double prevX = 0;
            double prevY = 0;
            bool first = true;

            for (int i = 0; i < numSides + 1; i++)
            {
                double angle = rotation + 2 * Math.PI * i / numSides;
                double x = centerX + radius * Math.Cos(angle);
                double y = centerY + radius * Math.Sin(angle);

                if (first)
                {
                    first = false;
                    prevX = x;
                    prevY = y;
                    continue;
                }

                yield return new Line(new PointXY(prevX, prevY), new PointXY(x, y));

                prevX = x;
                prevY = y;
            }
        }
    }
}
