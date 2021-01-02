﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tharga.Toolkit.Console;
using Tharga.Toolkit.Console.Commands;
using Tharga.Toolkit.Console.Commands.Base;
using Tharga.Toolkit.Console.Consoles;

namespace SaintCoinach.Cmd {
    class ConsoleProgressReporter : IProgress<Ex.Relational.Update.UpdateProgress> {

        #region IProgress<UpdateProgress> Members

        public void Report(Ex.Relational.Update.UpdateProgress value) {
            Console.WriteLine(value);
        }

        #endregion
    }
    class Program {
        private static void Main(string[] args) {
            var dataPath = "TODO: DataPath";//Properties.Settings.Default.DataPath;

            if (args.Length > 0) {
                dataPath = args[0];
                args = args.Skip(1).ToArray();
            }
            if (string.IsNullOrWhiteSpace(dataPath))
                dataPath = SearchForDataPaths().FirstOrDefault(p => System.IO.Directory.Exists(p));
            if (string.IsNullOrWhiteSpace(dataPath) || !System.IO.Directory.Exists(dataPath)) {
                Console.WriteLine($"Need data!  The path '{dataPath}' doesn't exist.");
                return;
            }

            var realm = new ARealmReversed(dataPath, @"SaintCoinach.History.zip", Ex.Language.English, @"app_data.sqlite");
            realm.Packs.GetPack(new IO.PackIdentifier("exd", IO.PackIdentifier.DefaultExpansion, 0)).KeepInMemory = true;

            Console.WriteLine("Game version: {0}", realm.GameVersion);
            Console.WriteLine("Definition version: {0}", realm.DefinitionVersion);
            
            if (!realm.IsCurrentVersion) {
                Console.Write("Update is available, perform update (Y/n)? ");
                //var updateQuery = Console.ReadLine();
                var updateQuery = "n";
                if (string.IsNullOrEmpty(updateQuery) || string.Equals("y", updateQuery, StringComparison.OrdinalIgnoreCase)) {
                    var stopWatch = new System.Diagnostics.Stopwatch();
                    stopWatch.Start();
                    realm.Update(true, new ConsoleProgressReporter());
                    stopWatch.Stop();
                    Console.WriteLine(stopWatch.Elapsed);
                } else
                    Console.WriteLine("Skipping update");
            }

            // TODO: Original
            //var cns = new Tharga.Toolkit.Console.Command.Base.ClientConsole();
            // TODO: Me running command directly for testing
            var exd = new Commands.AllExdCommand(realm);
            exd.WriteEvent += (s, e) => {
                Console.WriteLine($"{e.OutputLevel.ToString()}: {e.Message}");
            };

            exd.Invoke(null);

            var cns = new ClientConsole();
            var cmd = new RootCommand(cns);

            Setup(cmd, realm);
            (new CommandEngine(cmd)).Start(args);
        }

        static void Setup(RootCommand rootCmd, ARealmReversed realm) {
            var assembly = typeof(Program).Assembly;
            foreach (var t in assembly.GetTypes().Where(t => typeof(ActionCommandBase).IsAssignableFrom(t)))
            {
                var cons = t.GetConstructor(new[] { typeof(ARealmReversed) });
                rootCmd.RegisterCommand((ActionCommandBase)cons.Invoke(new[] { realm }));
            }
        }

        static string[] SearchForDataPaths() {
            const string gameFolder = "FINAL FANTASY XIV - A Realm Reborn";

            string programDir;
            if (Environment.Is64BitProcess)
                programDir = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            else
                programDir = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);

            return new string[] {
                System.IO.Path.Combine(programDir, "SquareEnix", gameFolder),
                System.IO.Path.Combine(@"D:\Games\SteamApps\common", gameFolder)
            };
        }
    }
}
