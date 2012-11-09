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
using NuProgram = NuGet.Program;
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

        public static void NuMain(StringBuilder b, string path, params string[] args)
        {
            var lastOut = Console.Out;
            var fileSystem = new PhysicalFileSystem(path ?? Directory.GetCurrentDirectory());
            try
            {
                Console.SetOut(new ConsoleWriter(b));
                RemoveOldFile(fileSystem);
                var program = new NuProgram();
                _initializeMethod.Invoke(program, new[] { fileSystem });
                HttpClient.DefaultCredentialProvider = new ConsoleCredentialProvider();
                foreach (var command2 in program.Commands)
                    program.Manager.RegisterCommand(command2);
                var command = (new CommandLineParser(program.Manager).ParseCommandLine(args) ?? program.HelpCommand);
                if (!NuProgram.ArgumentCountValid(command))
                {
                    var commandName = command.CommandAttribute.CommandName;
                    Console.WriteLine("InvalidArguments", commandName);
                    program.HelpCommand.ViewHelpForCommand(commandName);
                }
                else
                    command.Execute();
            }
            finally { Console.SetOut(lastOut); }
        }

        public static void NuMain(StringBuilder b, string path, Func<ICommandManager, ICommand> commandBuilder)
        {
            var lastOut = Console.Out;
            var fileSystem = new PhysicalFileSystem(path ?? Directory.GetCurrentDirectory());
            try
            {
                Console.SetOut(new ConsoleWriter(b));
                RemoveOldFile(fileSystem);
                var program = new NuProgram();
                _initializeMethod.Invoke(program, new[] { fileSystem });
                HttpClient.DefaultCredentialProvider = new ConsoleCredentialProvider();
                foreach (var command2 in program.Commands)
                    program.Manager.RegisterCommand(command2);
                var command = commandBuilder(program.Manager);
                if (!NuProgram.ArgumentCountValid(command))
                {
                    var commandName = command.CommandAttribute.CommandName;
                    Console.WriteLine("InvalidArguments", commandName);
                    program.HelpCommand.ViewHelpForCommand(commandName);
                }
                else
                    command.Execute();
            }
            finally { Console.SetOut(lastOut); }
        }


        private static void RemoveOldFile(IFileSystem fileSystem)
        {
            var path = typeof(Program).Assembly.Location + ".old";
            try { if (fileSystem.FileExists(path)) fileSystem.DeleteFile(path); }
            catch { }
        }
    }
}
