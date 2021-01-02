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
    public class ImageCommand : ActionCommandBase {
        private ARealmReversed _Realm;

        public ImageCommand(ARealmReversed realm)
            : base("image", "Export an image file.") {
            _Realm = realm;
        }

        public override void Invoke(string[] param) {
            try {
                if (_Realm.Packs.TryGetFile(string.Join(" ", param), out var file)) {
                    if (file is Imaging.ImageFile imgFile) {
                        var img = imgFile.GetImage();

                        var target = new FileInfo(Path.Combine(_Realm.GameVersion, file.Path));
                        if (!target.Directory.Exists)
                            target.Directory.Create();
                        var pngPath = target.FullName.Substring(0, target.FullName.Length - target.Extension.Length) + ".png";
                        img.Save(pngPath);
                    } else
                        OutputError(string.Format("File is not an image (actual: {0}).", file.CommonHeader.FileType));
                } else
                    OutputError(string.Format("File not found."));
            } catch (Exception e) {
                OutputError(e.Message);
            }
        }
    }
}
