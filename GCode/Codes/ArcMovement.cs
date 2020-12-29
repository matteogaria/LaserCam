//Copyright (c) 2020 Matteo Garia - https://github.com/matteogaria

// This program is free software; you can redistribute it and/or modify  it under the terms of the GPLv3 General Public License as published by  the Free Software Foundation; either version 3 of the License, or (at  your option) any later version.
// This program is distributed in the hope that it will be useful, but  WITHOUT ANY WARRANTY; without even the implied warranty of  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GPLv3  General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Text;

using GCode.Models;

namespace GCode.Codes
{
    public class CWArc : GCodeObject
    {
        public override string Code => "G2";

        public double EndX { get; }
        public double EndY { get; }
        public int FeedRate { get; }
        public double OffsetX { get; }
        public double OffsetY { get; }

        public override void Build(StringBuilder builder, int decimalPlaces)
        {
            builder.Append(Code);
            builder.Append("X");
            builder.Append(EndX.ToRoundedString(decimalPlaces));
            builder.Append("Y");
            builder.Append(EndY.ToRoundedString(decimalPlaces));
            if (OffsetX != 0.000)
            {
                builder.Append("I");
                builder.Append(OffsetX.ToRoundedString(decimalPlaces));
            }
            if (OffsetY != 0.000)
            {
                builder.Append("J");
                builder.Append(OffsetY.ToRoundedString(decimalPlaces));
            }
            if(FeedRate != -1)
            {
                builder.Append("F");
                builder.Append(FeedRate);
            }

        }

        public CWArc(double endX, double endY, double offsetX, double offsetY, int feedRate = -1)
        {
            EndX = endX;
            EndY = endY;
            FeedRate = feedRate;
            OffsetX = Math.Round(offsetX, 3);
            OffsetY = Math.Round(offsetY, 3);
        }
    }

    public class CCWArc : CWArc
    {
        public override string Code => "G3";

        public CCWArc(double endX, double endY, double offsetX, double offsetY, int feedRate = -1) : base(endX, endY, offsetX, offsetY, feedRate)
        { }
    }
}
