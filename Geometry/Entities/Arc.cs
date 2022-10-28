//Copyright (c) 2020 Matteo Garia - https://github.com/matteogaria

// This program is free software; you can redistribute it and/or modify  it under the terms of the GPLv3 General Public License as published by  the Free Software Foundation; either version 3 of the License, or (at  your option) any later version.
// This program is distributed in the hope that it will be useful, but  WITHOUT ANY WARRANTY; without even the implied warranty of  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GPLv3  General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;

namespace Geometry.Entities
{
    public class Arc : GeometryObject
    {
        public PointXY Center { get; protected set; }
        public double Radius { get; protected set; }
        public double TotalAngle { get; protected set; }
        public double StartAngle { get; protected set; }
        public double EndAngle { get; protected set; }
        public bool IsCW { get; protected set; }

        private PointXY startPoint;
        private PointXY endPoint;
        private double length;

        public Arc(PointXY center, double radius, double startAngle, double endAngle, bool isCW, string refName) : base(refName)
        {
            Create(center, radius, startAngle, endAngle, isCW);
        }

        public Arc(PointXY center, double radius, double startAngle, double endAngle, bool isCW, GeometryObject reference) : base(reference)
        {
            Create(center, radius, startAngle, endAngle, isCW);
        }

        public Arc(PointXY center, double radius, double startAngle, double endAngle, bool isCW) : base(string.Empty)
        {
            Create(center, radius, startAngle, endAngle, isCW);
        }

        protected void Create(PointXY center, double radius, double startAngle, double endAngle, bool isCW)
        {
            Center = center;
            Radius = radius;
            StartAngle = startAngle;
            EndAngle = endAngle;
            IsCW = isCW;

            TotalAngle = Math.Abs(endAngle - startAngle);
            if (isCW && startAngle < endAngle || !isCW && startAngle > endAngle)
                TotalAngle = 360 - TotalAngle;

            length = 2 * Math.PI * Radius * (TotalAngle) / 360;

            startPoint = center.Move(MathUtils.Misc.ToRad(startAngle), radius);
            endPoint = center.Move(MathUtils.Misc.ToRad(endAngle), radius);
        }
        public override PointXY GetStartPoint() => startPoint;
        public override PointXY GetEndPoint() => endPoint;
        public override double GetLength() => length;

        public override void Reverse()
        {
            PointXY newStartPoint = endPoint;
            PointXY newEndPoint = startPoint;
            double newStartAngle = EndAngle;
            double newEndAngle = StartAngle;

            this.startPoint = newStartPoint;
            this.endPoint = newEndPoint;
            this.StartAngle = newStartAngle;
            this.EndAngle = newEndAngle;
            IsCW = !IsCW;
        }

        public IEnumerable<Arc> SplitAt(double length)
        {
            double sectionAngle = MathUtils.Misc.ToDegree(length / Radius);

            List<Arc> arcs = new();
            if (IsCW)
            {
                arcs.Add(new Arc(Center, Radius, StartAngle, StartAngle - sectionAngle, true, this));
                arcs.Add(new Arc(Center, Radius, StartAngle - sectionAngle, EndAngle, true, this));
            }
            else
            {
                arcs.Add(new Arc(Center, Radius, StartAngle, StartAngle + sectionAngle, false, this));
                arcs.Add(new Arc(Center, Radius, StartAngle + sectionAngle, EndAngle, false, this));
            }

            return arcs;

        }
    }
}
