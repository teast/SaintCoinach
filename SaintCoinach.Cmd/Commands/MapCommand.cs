﻿using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tharga.Toolkit.Console;
using Tharga.Toolkit.Console.Commands;
using Tharga.Toolkit.Console.Commands.Base;

#pragma warning disable CS1998

namespace SaintCoinach.Cmd.Commands {
    public class MapCommand : ActionCommandBase {
        private ARealmReversed _Realm;

        public MapCommand(ARealmReversed realm)
            : base("maps", "Export all map images.") {
            _Realm = realm;
        }

        public override void Invoke(string[] param) {
            var format = ImageFormat.Png;

            if (param?.Length > 0) {
                if (param.Contains("jpg"))
                    format = ImageFormat.Jpeg;
                else if (param.Contains("png"))
                    format = ImageFormat.Png;
                else
                    OutputError(string.Format("Invalid map format " + string.Join(" ", param)));
            }

            var c = 0;
            var allMaps = _Realm.GameData.GetSheet<SaintCoinach.Xiv.Map>()
                .Where(m => m.PlaceName != null);

            var fileSet = new Dictionary<string, int>();
            foreach (var map in allMaps) {
                var img = map.MediumImage;
                if (img == null)
                    continue;

                var outPathSb = new StringBuilder("ui/map/");
                var territoryName = map.TerritoryType?.Name?.ToString();
                if (!string.IsNullOrEmpty(territoryName)) {
                    if (territoryName.Length < 3) {
                        outPathSb.AppendFormat("{0}/", territoryName);
                    }
                    else {
                        outPathSb.AppendFormat("{0}/", territoryName.Substring(0, 3));
                    }

                    outPathSb.AppendFormat("{0} - ", territoryName);
                }
                outPathSb.AppendFormat("{0}", ToPathSafeString(map.PlaceName.Name.ToString()));
                if (map.LocationPlaceName != null && map.LocationPlaceName.Key != 0 && !map.LocationPlaceName.Name.IsEmpty)
                    outPathSb.AppendFormat(" - {0}", ToPathSafeString(map.LocationPlaceName.Name.ToString()));
                var mapKey = outPathSb.ToString();
                fileSet.TryGetValue(mapKey, out int mapIndex);
                if (mapIndex > 0) {
                    outPathSb.AppendFormat(" - {0}", mapIndex);
                }
                fileSet[mapKey] = mapIndex + 1;
                outPathSb.Append(FormatToExtension(format));

                var outFile = new FileInfo(Path.Combine(_Realm.GameVersion, outPathSb.ToString()));
                if (!outFile.Directory.Exists)
                    outFile.Directory.Create();

                img.Save(outFile.FullName, format);
                ++c;
            }
            OutputInformation(string.Format("{0} maps saved", c));
        }
        
        static string FormatToExtension(ImageFormat format) {
            if (format == ImageFormat.Png)
                return ".png";
            if (format == ImageFormat.Jpeg)
                return ".jpg";

            throw new NotImplementedException();
        }

        static string ToPathSafeString(string input, char invalidReplacement = '_') {
            var sb = new StringBuilder(input);
            var invalid = Path.GetInvalidFileNameChars();
            foreach (var c in invalid)
                sb.Replace(c, invalidReplacement);
            return sb.ToString();
        }
    }
}
