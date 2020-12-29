//Copyright (c) 2020 Matteo Garia - https://github.com/matteogaria

// This program is free software; you can redistribute it and/or modify  it under the terms of the GPLv3 General Public License as published by  the Free Software Foundation; either version 3 of the License, or (at  your option) any later version.
// This program is distributed in the hope that it will be useful, but  WITHOUT ANY WARRANTY; without even the implied warranty of  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GPLv3  General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;

using netDxf;

namespace DxfTools
{
    public static class Extensions
    {
        public static void ExplodeLwPoly(this DxfDocument doc)
        {
            List<netDxf.Entities.EntityObject> objectsToAdd = new List<netDxf.Entities.EntityObject>();
            List<netDxf.Entities.EntityObject> objectsToRemove = new List<netDxf.Entities.EntityObject>();

            foreach (var lwpoly in doc.LwPolylines)
            {
                objectsToAdd.AddRange(lwpoly.Explode());
                objectsToRemove.Add(lwpoly);
            }

            doc.RemoveEntity(objectsToRemove);
            doc.AddEntity(objectsToAdd);
        }

        public static void AddOrClone(this DxfDocument doc, IEnumerable<netDxf.Entities.EntityObject> entities)
        {
            foreach (var entity in entities)
            {
                if (entity.Owner == null)
                    doc.AddEntity(entity);
                else
                    doc.AddEntity((netDxf.Entities.EntityObject)entity.Clone());
            }
        }
    }
}

namespace Geometry.Entities
{
    public static class PointExtensions
    {
        public static PointXY ToPoint(this Vector3 vector) => new PointXY(vector.X, vector.Y);

        public static Vector3 ToVector(this PointXY point) => new Vector3(point.X, point.Y, 0);

    }
}
