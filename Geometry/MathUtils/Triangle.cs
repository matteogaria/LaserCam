//Copyright (c) 2020 Matteo Garia - https://github.com/matteogaria

// This program is free software; you can redistribute it and/or modify  it under the terms of the GPLv3 General Public License as published by  the Free Software Foundation; either version 3 of the License, or (at  your option) any later version.
// This program is distributed in the hope that it will be useful, but  WITHOUT ANY WARRANTY; without even the implied warranty of  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GPLv3  General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;

namespace Geometry.MathUtils
{
    public static class Triangle
    {
        public static double Hypotenuse(double a, double b) => Math.Abs(Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2)));

        public static double Opposite(double hypotenuse, double angleRad) => hypotenuse * Math.Sin(angleRad);

        public static double Adiacent(double hypotenuse, double angleRad) => hypotenuse* Math.Cos(angleRad);

    }
}
