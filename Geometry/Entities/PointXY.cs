//Copyright (c) 2020 Matteo Garia - https://github.com/matteogaria

// This program is free software; you can redistribute it and/or modify  it under the terms of the GPLv3 General Public License as published by  the Free Software Foundation; either version 3 of the License, or (at  your option) any later version.
// This program is distributed in the hope that it will be useful, but  WITHOUT ANY WARRANTY; without even the implied warranty of  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GPLv3  General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Drawing;

namespace Geometry.Entities
{
    public struct PointXY
    {
        public double X { get; }
        public double Y { get; }
        public double DistanceFromOrigin => MathUtils.Triangle.Hypotenuse(X, Y);

        public PointXY(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static PointXY InvalidPoint => new PointXY(double.NaN, double.NaN);

        public static bool IsSamePoint(PointXY a, PointXY b, double tolerance)
            => MathUtils.Misc.CompareDouble(a.X, b.X, tolerance) && MathUtils.Misc.CompareDouble(a.Y, b.Y, tolerance);

        public static double Distance(PointXY a, PointXY b)
            => MathUtils.Triangle.Hypotenuse(a.X - b.X, a.Y - b.Y);

        public static bool operator ==(PointXY a, PointXY b)
            => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(PointXY a, PointXY b)
            => !(a == b);

        public override string ToString()
        {
            return $"X:{X} Y:{Y}";
        }
    }

    public static class PointExtensions
    {
        public static PointXY Zero(this PointXY p, double tolerance)
        {
            double x = Math.Abs(p.X) < tolerance ? 0 : p.X;
            double y = Math.Abs(p.Y) < tolerance ? 0 : p.Y;
            return new PointXY(x, y);
        }

        public static PointXY Offset(this PointXY p, double x, double y)
            => new PointXY(p.X + x, p.Y + y);

        public static PointXY Move(this PointXY p, double directionRad, double distance)
        {
            double x = p.X + distance * Math.Cos(directionRad);
            double y = p.Y + distance * Math.Sin(directionRad);
            return new PointXY(x, y);
        }
    }
}