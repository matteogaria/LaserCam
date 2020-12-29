//Copyright (c) 2020 Matteo Garia - https://github.com/matteogaria

// This program is free software; you can redistribute it and/or modify  it under the terms of the GPLv3 General Public License as published by  the Free Software Foundation; either version 3 of the License, or (at  your option) any later version.
// This program is distributed in the hope that it will be useful, but  WITHOUT ANY WARRANTY; without even the implied warranty of  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GPLv3  General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;

using Geometry.Entities;

namespace Geometry.MathUtils
{
    public static class Misc
    {

        public static int CeilInt(int dividend, int divisor)
            => (dividend + divisor - 1) / divisor;

        public static int Closest(int number, IEnumerable<int> list)
            => list.Aggregate((x, y) => Math.Abs(x-number) < Math.Abs(y-number) ? x : y);

        public static bool CompareDouble(double d1, double d2, double tolerance)
        {
            return Math.Abs(d2 - d1) < tolerance;
        }

        public static double ToRad(double degree) => degree * Math.PI / 180.0;

        public static double ToDegree(double rad) => rad * 180.0 / Math.PI;

        public static bool IsArcCW(PointXY center, PointXY start, PointXY end)
        {
            double c = (start.X - center.X) * (end.Y - center.Y) - (start.Y - center.Y) * (end.X - center.X);
            return c < 0;
        }

        public static bool IsArcCW(double startAngle, double endAngle)
        {
            return startAngle > endAngle;
        }
    }
}
