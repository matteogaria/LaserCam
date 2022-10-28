//Copyright (c) 2020 Matteo Garia - https://github.com/matteogaria

// This program is free software; you can redistribute it and/or modify  it under the terms of the GPLv3 General Public License as published by  the Free Software Foundation; either version 3 of the License, or (at  your option) any later version.
// This program is distributed in the hope that it will be useful, but  WITHOUT ANY WARRANTY; without even the implied warranty of  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GPLv3  General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;

namespace Geometry.Entities
{
    public class Circle : GeometryObject
    {
        public PointXY Center { get; protected set; }
        public double Radius { get; protected set; }
        public double Circumference { get; protected set; }

        public Circle(PointXY center, double radius, string refName) : base(refName)
        {
            Create(center, radius);
        }
        public Circle(PointXY center, double radius, GeometryObject reference) : base(reference)
        {
            Create(center, radius);
        }
        public Circle(PointXY center, double radius) : base(string.Empty)
        {
            Create(center, radius);
        }

        protected void Create(PointXY center, double radius)
        {
            Center = center;
            Radius = radius;
            Circumference = 2 * Math.PI * radius;
        }

        public override PointXY GetStartPoint() => Center.Move( 0, Radius);

        public override PointXY GetEndPoint() => GetStartPoint();
        
        public override double GetLength() => Circumference;

        public override void Reverse()
        {
        }

        public IEnumerable<Arc> SplitToArcs(double maxSectionLen)
        {
            // calculating the number of sections to generate
            int sections = (int)Math.Ceiling(Circumference / maxSectionLen);

            List<Arc> results = new List<Arc>();

            //return an empty list if no splitting is needed
            if (sections == 1)
                return results;

            // calculating the angle of each section
            double angle = 360.0 / sections;

            //calculating the nearest angle, to get only integer angles
            int closestAngle = MathUtils.Misc.Closest((int)angle, MathUtils.Constants.DegreesDivisors);

            // generating arc entities
            for (int i = 0; i < 360; i += closestAngle)
            {
                //circles angle is always defined ccw
                Arc arc = new Arc(Center, Radius, i, i + closestAngle, false, this);
                results.Add(arc);
            }

            return results;

        }
    }
}
