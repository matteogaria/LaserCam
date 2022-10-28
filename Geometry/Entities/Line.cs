//Copyright (c) 2020 Matteo Garia - https://github.com/matteogaria

// This program is free software; you can redistribute it and/or modify  it under the terms of the GPLv3 General Public License as published by  the Free Software Foundation; either version 3 of the License, or (at  your option) any later version.
// This program is distributed in the hope that it will be useful, but  WITHOUT ANY WARRANTY; without even the implied warranty of  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GPLv3  General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;

namespace Geometry.Entities
{
    public class Line : GeometryObject
    {
        public double Slope { get; protected set; }
        public double Intercept { get; protected set; }
        public double Angle { get; protected set; }
        public double AngleRad { get; protected set; }

        private PointXY startPoint;
        private PointXY endPoint;
        private double length;

        public Line(PointXY startPoint, PointXY endPoint) : base(string.Empty)
        {
            Create(startPoint, endPoint);
        }

        public Line(PointXY startPoint, PointXY endPoint, GeometryObject reference) : base(reference)
        {
            Create(startPoint, endPoint);
        }

        public Line(PointXY startPoint, PointXY endPoint, string refName) : base(refName)
        {
            Create(startPoint, endPoint);
        }

        protected void Create(PointXY startPoint, PointXY endPoint)
        {
            Slope = (endPoint.Y - startPoint.Y) / (endPoint.X - startPoint.X);
            Intercept = -(Slope * startPoint.X) + startPoint.Y;
            length = MathUtils.Triangle.Hypotenuse(endPoint.X - startPoint.X, endPoint.Y - startPoint.Y);

            if (startPoint.X == endPoint.X)
            {
                Angle = (startPoint.Y < endPoint.Y) ? 90.0 : -90.0;
                AngleRad = MathUtils.Misc.ToRad(Angle);
            }
            else
            {
                AngleRad = Math.Atan(Slope);
                Angle = MathUtils.Misc.ToDegree(AngleRad);
            }

            this.startPoint = startPoint;
            this.endPoint = endPoint;
        }

        public double GetY(double x) => Slope * x + Intercept;

        public double GetX(double y) => (y - Intercept) / Slope;

        public override PointXY GetStartPoint() => startPoint;

        public override PointXY GetEndPoint() => endPoint;
        public override double GetLength() => length;

        public override void Reverse()
        {
            PointXY newStartPoint = endPoint;
            PointXY newEndPoint = startPoint;

            Create(newStartPoint, newEndPoint);
        }

        public IEnumerable<Line> SplitAt(double length, double tolerance)
        {
            List<Line> lines = new List<Line>();
            PointXY point;

            if (StartPoint.X <= EndPoint.X)
                point = StartPoint.Move(AngleRad, length).Zero(tolerance);
            else
                point = StartPoint.Move(AngleRad, -length).Zero(tolerance);

            lines.Add(new Line(StartPoint, point, this));
            lines.Add(new Line(point, EndPoint, this));

            return lines;
        }
    }
}