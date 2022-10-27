using System;
using System.Collections.Generic;

using Geometry.Entities;
using Geometry.MathUtils;

namespace Geometry
{
    public class ShapeBuilder
    {
        public static IEnumerable<GeometryObject> BuildRegularPolygon(double numSides, double radius)
        {
            double centerX = radius;
            double centerY = radius / (2 * Math.Tan(Misc.ToRad(180 / numSides)));
            double prevX = 0;
            double prevY = 0;
            bool first = true;

            //double rotation = Misc.ToRad(n % 2 == 0 ? 45 : 90);
            double rotation = numSides switch
            {
                6 or 10 => 0,
                _ => Misc.ToRad(numSides % 2 == 0 ? (180 / numSides) : 90)
            };

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
