using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tharga.Toolkit.Console;
using Tharga.Toolkit.Console.Commands;
using Tharga.Toolkit.Console.Commands.Base;

#pragma warning disable CS1998

namespace SaintCoinach.Cmd.Commands {
    using Ex;

    public class LanguageCommand : ActionCommandBase {
        private ARealmReversed _Realm;

        public LanguageCommand(ARealmReversed realm)
            : base("lang", "Change the language.") {
            _Realm = realm;
        }

        public override void Invoke(string[] param) {
            if (param == null || param.Length == 0) {
                OutputInformation(string.Format("Current language: {0}", _Realm.GameData.ActiveLanguage));
                return;
            }
            var paramList = string.Join(" ", param).Trim();
            if (!Enum.TryParse<Language>(paramList, out var newLang)) {
                newLang = LanguageExtensions.GetFromCode(paramList);
                if (newLang == Language.Unsupported) {
                    OutputError(string.Format("Unknown language."));
                }
            }
            _Realm.GameData.ActiveLanguage = newLang;
        }
    }
}
