using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tharga.Toolkit.Console;
using Tharga.Toolkit.Console.Commands;
using Tharga.Toolkit.Console.Commands.Base;

using SaintCoinach;
using SaintCoinach.Ex;
using SaintCoinach.Xiv;

#pragma warning disable CS1998

namespace SaintCoinach.Cmd.Commands {
    public class AllExdCommand : ActionCommandBase {
        private ARealmReversed _Realm;

        public AllExdCommand(ARealmReversed realm)
            : base("allexd", "Export all data (default), or only specific data files, seperated by spaces; including all languages.") {
            _Realm = realm;
        }

        public override void Invoke(string[] param)
        {
            const string CsvFileFormat = "exd-all/{0}{1}.csv";

            IEnumerable<string> filesToExport;

            if (param == null || param.Length == 0)
                filesToExport = _Realm.GameData.AvailableSheets;
            else
                filesToExport = param.Select(_ => _Realm.GameData.FixName(_));

            var successCount = 0;
            var failCount = 0;
            foreach (var name in filesToExport) {
                var sheet = _Realm.GameData.GetSheet(name);
                foreach(var lang in sheet.Header.AvailableLanguages) {
                    var code = lang.GetCode();
                    if (code.Length > 0)
                        code = "." + code;
                    var target = new FileInfo(Path.Combine(_Realm.GameVersion, string.Format(CsvFileFormat, name, code)));
                    try {

                        if (!target.Directory.Exists)
                            target.Directory.Create();

                        ExdHelper.SaveAsCsv(sheet, lang, target.FullName, false);

                        ++successCount;
                    } catch (Exception e) {
                        OutputError(string.Format("Export of {0} failed: {1}", name, e.Message));
                        try { if (target.Exists) { target.Delete(); } } catch { }
                        ++failCount;
                    }
                }
                
            }
            OutputInformation(string.Format("{0} files exported, {1} failed", successCount, failCount));
        }
    }
}
