//Copyright (c) 2020 Matteo Garia - https://github.com/matteogaria

// This program is free software; you can redistribute it and/or modify  it under the terms of the GPLv3 General Public License as published by  the Free Software Foundation; either version 3 of the License, or (at  your option) any later version.
// This program is distributed in the hope that it will be useful, but  WITHOUT ANY WARRANTY; without even the implied warranty of  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GPLv3  General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;

namespace Geometry.Entities
{
    public class PointXY
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double DistanceFromOrigin => MathUtils.Triangle.Hypotenuse(X, Y);

        public PointXY(double x, double y)
        {
            X = x;
            Y = y;
        }

        public PointXY Normalize(double tolerance)
        {
            if (Math.Abs(X) < tolerance)
                X = 0;
            if (Math.Abs(Y) < tolerance)
                Y = 0;

            return this;
        }

        public void Move(double directionRad, double distance)
        {
            X += distance * Math.Cos(directionRad);
            Y += distance * Math.Sin(directionRad);
        }

        public static bool IsSamePoint(PointXY a, PointXY b, double tolerance)
            => MathUtils.Misc.CompareDouble(a.X, b.X, tolerance) && MathUtils.Misc.CompareDouble(a.Y, b.Y, tolerance);

        public static double Distance(PointXY a, PointXY b)
            => MathUtils.Triangle.Hypotenuse(a.X - b.X, a.Y - b.Y);

        public static PointXY MovePoint(PointXY point, double directionRad, double distance)
        {
            double x = point.X + distance * Math.Cos(directionRad);
            double y = point.Y + distance * Math.Sin(directionRad);
            PointXY retVal = new PointXY(x, y);

            return retVal;
        }

        public static bool operator ==(PointXY a, PointXY b)
            => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(PointXY a, PointXY b)
            => !(a == b);

        public override string ToString()
        {
            return $"X:{X} Y:{Y}";
        }
    }
}
