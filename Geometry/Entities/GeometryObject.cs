//Copyright (c) 2020 Matteo Garia - https://github.com/matteogaria

// This program is free software; you can redistribute it and/or modify  it under the terms of the GPLv3 General Public License as published by  the Free Software Foundation; either version 3 of the License, or (at  your option) any later version.
// This program is distributed in the hope that it will be useful, but  WITHOUT ANY WARRANTY; without even the implied warranty of  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GPLv3  General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program.  If not, see <https://www.gnu.org/licenses/>.

namespace Geometry.Entities
{
    public abstract class GeometryObject
    {
        public string RefName { get; }
        public PointXY StartPoint { get => GetStartPoint(); }
        public PointXY EndPoint { get => GetEndPoint(); }

        public double Length { get => GetLength(); }

        public GeometryObject(GeometryObject reference)
        {
            if (reference != null)
                RefName = reference.RefName;
        }

        public GeometryObject(string refName)
        {
            RefName = refName;
        }

        public abstract PointXY GetStartPoint();
        public abstract PointXY GetEndPoint();
        public abstract double GetLength();
        public abstract void Reverse();

        public static bool AreContinuous(GeometryObject a, GeometryObject b, double tolerance)
        {
            return
                PointXY.IsSamePoint(a.StartPoint, b.StartPoint, tolerance) ||
                PointXY.IsSamePoint(a.StartPoint, b.EndPoint, tolerance) ||
                PointXY.IsSamePoint(a.EndPoint, b.StartPoint, tolerance) ||
                PointXY.IsSamePoint(a.EndPoint, b.EndPoint, tolerance);
        }
    }
}
