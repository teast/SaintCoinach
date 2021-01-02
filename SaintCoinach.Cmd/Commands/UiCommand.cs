using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tharga.Toolkit.Console;
using Tharga.Toolkit.Console.Commands;
using Tharga.Toolkit.Console.Commands.Base;

#pragma warning disable CS1998

namespace SaintCoinach.Cmd.Commands {
    public class UiCommand : ActionCommandBase {
        const string UiImagePathFormat = "ui/icon/{0:D3}000{1}/{2:D6}.tex";
        static readonly string[] UiVersions = new string[] {
            "",
            "/en",
            "/ja",
            "/fr",
            "/de",
            "/hq"
        };

        private ARealmReversed _Realm;

        public UiCommand(ARealmReversed realm)
            : base("ui", "Export all, a single, or a range of UI icons.") {
            _Realm = realm;
        }

        public override void Invoke(string[] param) {
            var min = 0;
            var max = 999999;

            if (param?.Length > 0) {
                var splitParam = param;

                if (splitParam.Length == 1) {
                    if (int.TryParse(splitParam[0], out var parsed))
                        min = max = parsed;
                    else {
                        OutputError(string.Format("Failed to parse parameters."));
                        return;
                    }
                } else if (splitParam.Length == 2) {
                    if (!int.TryParse(splitParam[0], out min) || !int.TryParse(splitParam[1], out max)) {
                        OutputError(string.Format("Failed to parse parameters."));
                        return;
                    }

                    if (max < min) {
                        OutputError(string.Format("Invalid parameters."));
                        return;
                    }
                } else {
                    OutputError(string.Format("Failed to parse parameters."));
                    return;
                }
            }

            var count = 0;
            for (int i = min; i <= max; ++i) {
                try {
                    count += Process(i);
                } catch (Exception e) {
                    OutputError(string.Format("{0:D6}: {1}", i, e.Message));
                }
            }
            OutputInformation(string.Format("{0} images processed", count));

            return;
        }

        private int Process(int i) {
            var count = 0;
            foreach (var v in UiVersions) {
                if (Process(i, v))
                    ++count;
            }
            return count;
        }
        private bool Process(int i, string version) {
            var filePath = string.Format(UiImagePathFormat, i / 1000, version, i);

            if (_Realm.Packs.TryGetFile(filePath, out var file)) {
                if (file is Imaging.ImageFile imgFile) {
                    var img = imgFile.GetImage();

                    var target = new FileInfo(Path.Combine(_Realm.GameVersion, file.Path));
                    if (!target.Directory.Exists)
                        target.Directory.Create();
                    var pngPath = target.FullName.Substring(0, target.FullName.Length - target.Extension.Length) + ".png";
                    img.Save(pngPath);

                    return true;
                } else {
                    OutputError(string.Format("{0} is not an image.", filePath));
                }
            }
            return false;
        }
    }
}
