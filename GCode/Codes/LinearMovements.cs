//Copyright (c) 2020 Matteo Garia - https://github.com/matteogaria

// This program is free software; you can redistribute it and/or modify  it under the terms of the GPLv3 General Public License as published by  the Free Software Foundation; either version 3 of the License, or (at  your option) any later version.
// This program is distributed in the hope that it will be useful, but  WITHOUT ANY WARRANTY; without even the implied warranty of  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GPLv3  General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Text;
using System.Globalization;

namespace GCode.Codes
{
    public class RapidMove : Models.GCodeObject
    {
        public override string Code => "G0";

        public double X { get; }
        public double Y { get; }

        public override void Build(StringBuilder builder, int decimalPlaces)
        {
            builder.Append(Code);
            builder.Append("X");
            builder.Append(X.ToRoundedString(decimalPlaces));
            builder.Append("Y");
            builder.Append(Y.ToString("0.000", CultureInfo.InvariantCulture));
        }

        public RapidMove(double x, double y)
        {
            X = x;
            Y = y;
        }

    }

    public class LinearMove : RapidMove
    {
        public override string Code => "G1";

        public int FeedRate { get; }

        public override void Build(StringBuilder builder, int decimalPlaces)
        {
            base.Build(builder, decimalPlaces);
            if (FeedRate > 0)
            {
                builder.Append("F");
                builder.Append(FeedRate);
            }

        }

        public LinearMove(double x, double y, int feedRate = -1) : base(x, y)
        {
            FeedRate = feedRate;
        }
    }
}
