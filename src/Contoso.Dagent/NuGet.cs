#region License
/*
The MIT License

Copyright (c) 2008 Sky Morey

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/
#endregion
using System;
using System.IO;
using System.Reflection;
using System.Text;
using NuGet;
using NuGet.Commands;
using NuGet.Common;
using NuProgram = NuGet.Program;
using NuConsole = NuGet.Common.Console;
using SysConsole = System.Console;
namespace Contoso
{
    public class NuGet
    {
        private static readonly MethodInfo _initializeMethod = typeof(NuProgram).GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Instance);

        private class ConsoleWriter : TextWriter
        {
            private StringBuilder _b;

            public ConsoleWriter(StringBuilder b) { _b = b; }

            public override void Write(char value)
            {
                base.Write(value);
                _b.Append(value.ToString());
            }

            public override Encoding Encoding
            {
                get { return Encoding.UTF8; }
            }
        }

        public static void NuMain(StringBuilder b, string path, params string[] args) { NuMain(b, path, m => new CommandLineParser(m).ParseCommandLine(args)); }
        public static void NuMain(StringBuilder b, string path, Func<ICommandManager, ICommand> commandBuilder)
        {
            var console = new NuConsole();
            var fileSystem = new PhysicalFileSystem(path ?? Directory.GetCurrentDirectory());
            var lastOut = SysConsole.Out;
            try
            {
                SysConsole.SetOut(new ConsoleWriter(b));
                //
                RemoveOldFile(fileSystem);
                var program = new NuProgram();
                _initializeMethod.Invoke(program, new[] { fileSystem });
                //HttpClient.DefaultCredentialProvider = new ConsoleCredentialProvider();
                foreach (var command in program.Commands)
                    program.Manager.RegisterCommand(command);
                var command2 = (commandBuilder(program.Manager) ?? program.HelpCommand);
                if (!NuProgram.ArgumentCountValid(command2))
                {
                    var commandName = command2.CommandAttribute.CommandName;
                    console.WriteLine("InvalidArguments", new object[] { commandName });
                    program.HelpCommand.ViewHelpForCommand(commandName);
                }
                else
                {
                    SetConsoleInteractivity(console, command2 as Command);
                    command2.Execute();
                }
            }
            finally { SysConsole.SetOut(lastOut); }
        }

        private static void RemoveOldFile(IFileSystem fileSystem)
        {
            var path = typeof(Program).Assembly.Location + ".old";
            try { if (fileSystem.FileExists(path)) fileSystem.DeleteFile(path); }
            catch { }
        }

        private static void SetConsoleInteractivity(IConsole console, Command command)
        {
            var environmentVariable = Environment.GetEnvironmentVariable("NUGET_EXE_NO_PROMPT");
            var str2 = Environment.GetEnvironmentVariable("VisualStudioVersion");
            console.IsNonInteractive = (!string.IsNullOrEmpty(environmentVariable) || !string.IsNullOrEmpty(str2)) || (command != null && command.NonInteractive);
            if (command != null)
                console.Verbosity = command.Verbosity;
        }
    }
}
