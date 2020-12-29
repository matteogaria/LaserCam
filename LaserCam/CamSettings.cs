//Copyright (c) 2020 Matteo Garia - https://github.com/matteogaria

// This program is free software; you can redistribute it and/or modify  it under the terms of the GPLv3 General Public License as published by  the Free Software Foundation; either version 3 of the License, or (at  your option) any later version.
// This program is distributed in the hope that it will be useful, but  WITHOUT ANY WARRANTY; without even the implied warranty of  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GPLv3  General Public License for more details.
// You should have received a copy of the GNU General Public License along with this program.  If not, see <https://www.gnu.org/licenses/>.

using GCode.Models;
using Geometry.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LaserCam
{
    public class CamSettings
    {
        public string Name { get; set; }
        public int DecimalPlaces { get; set; }
        public double PointTolerance { get; set; }

        public List<LayerParameters> Parameters { get; set; } = new();

        public GCodeEngineSettings GetGCodeSettings()
        {
            return new GCodeEngineSettings
            {
                DecimalPlaces = this.DecimalPlaces,
                LayerSettings = Parameters.Select(p => new GCodeLayerSettings
                {
                    RefName = p.Layer,
                    FeedRate = p.FeedRate,
                    Passes = p.Passes,
                    Mode = p.Mode,
                    Power = p.Power,
                    PointTolerance = this.PointTolerance
                }).ToList()
            };
        }

        public IEnumerable<ShapeCreationSettings> GetShapeSettings()
        {
            return Parameters.Select(p => new ShapeCreationSettings
            {
                LayerName = p.Layer,
                SplitGeometries = p.SplitGeometries,
                SectionLength = p.SectionLength,
                SectionTolerance = p.SectionTolerance,
                PointTolerance = this.PointTolerance,
                Ordered = true
            });
        }

        public static List<CamSettings> Load(string filename)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters ={
                    new JsonStringEnumConverter()
                }
            };

            string content = File.ReadAllText(filename);
            List<CamSettings> settings = JsonSerializer.Deserialize<List<CamSettings>>(content, options);
            return settings;
        }

        public static void Save(string filename, IEnumerable<CamSettings> settings)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters ={
                    new JsonStringEnumConverter()
                }
            };

            string content = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(filename, content);
        }
    }

    public class LayerParameters
    {
        public string Layer { get; set; }
        public bool SplitGeometries { get; set; }
        public double SectionLength { get; set; }
        public double SectionTolerance { get; set; }
        public int FeedRate { get; set; }
        public int Power { get; set; }
        public int Passes { get; set; }
        public LaserMode Mode { get; set; }
    }
}
