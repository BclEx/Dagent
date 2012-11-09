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
using System.Abstract;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using Contoso.Bus.NugetMessages;
using NuGet;
using NuGet.Commands;
using NuGet.Common;
using Contoso.Nuget;
using System.Diagnostics;
namespace Contoso.NuGetCommands
{
    [Command(typeof(Local), "deploy", "DeployCommandDescription", MinArgs = 2, MaxArgs = 3, UsageDescriptionResourceName = "DeployCommandUsageDescription", UsageSummaryResourceName = "DeployCommandUsageSummary", UsageExampleResourceName = "DeployCommandUsageExamples")]
    public class DeployCommand : Command
    {
        static DeployCommand() { Runtime.Ensure(); }

        [ImportingConstructor]
        public DeployCommand(IPackageAgentProvider packageRemoteProvider, ISettings settings)
        {
            RemoteProvider = packageRemoteProvider;
            Settings = settings;
        }

        public override void ExecuteCommand()
        {
            var agent = base.Arguments[0];
            var packageId = base.Arguments[1];
            string defaultEmail;
            agent = RemoteProvider.ResolveAndValidateAgent(agent, out defaultEmail);
            var remoteApiKey = GetRemoteApiKey(agent, true);
            if (string.IsNullOrEmpty(remoteApiKey))
                throw new CommandLineException(Local.NoAgentApiKeyFound, new[] { Utility.GetRemoteDisplayName(agent) });
            //if (!System.Messaging.MessageQueue.Exists(remoteQueue))
            //    throw new CommandLineException(Local.NoRemoteQueueFound, new object[] { remoteQueue });
            //
            DeployMessage.Item[] items;
            bool fromSpec;
            if (Path.GetFileName(packageId).EndsWith(PackageReferenceRepository.PackageReferenceFile, StringComparison.OrdinalIgnoreCase))
            {
                if (IncludeDependency)
                    throw new CommandLineException("Message needed");
                fromSpec = true;
                Prerelease = true;
                items = DeployPackagesFromConfigFile(GetPackageReferenceFile(packageId));
                if (items == null)
                {
                    Console.WriteLine(Local.DeployCommandNoItemsFound, packageId);
                    return;
                }
            }
            else
            {
                fromSpec = !IncludeDependency;
                items = new[] { new DeployMessage.Item { PackageId = packageId, Version = Version } };
            }
            var bus = ServiceBusManager.Current;
            DeployReply.WaitState waitState = null;
            if (Wait)
                waitState = new DeployReply.WaitState
                {
                    Success = b => Console.WriteLine(b),
                    Failure = () => Console.WriteLine("didn't get in time {0}, {1}", agent, packageId),
                };
            bus.Send(agent, new DeployMessage
            {
                ApiKey = remoteApiKey,
                WantReply = Wait,
                FromSpec = fromSpec,
                Items = items,
                ExcludeVersion = !IncludeVersion,
                Prerelease = Prerelease,
                Email = Email ?? defaultEmail,
                Project = Project,
            });
            Console.WriteLine(Local.DeployCommandSent, agent, packageId);
            if (waitState != null)
            {
                Console.WriteLine("Waiting for reply...");
                waitState.DoWait();
            }
        }

        private string GetRemoteApiKey(string remote, bool throwIfNotFound = true)
        {
            if (!string.IsNullOrEmpty(ApiKey))
                return ApiKey;
            string str = null;
            if (Arguments.Count > 2)
                str = Arguments[2];
            if (string.IsNullOrEmpty(str))
                str = Utility.GetRemoteApiKey(Settings, remote, throwIfNotFound);
            return str;
        }

        protected virtual PackageReferenceFile GetPackageReferenceFile(string path) { return new PackageReferenceFile(Path.GetFullPath(path)); }

        private DeployMessage.Item[] DeployPackagesFromConfigFile(PackageReferenceFile file)
        {
            var source = file.GetPackageReferences().ToList();
            if (!source.Any())
                return null;
            var list = new List<DeployMessage.Item>();
            foreach (PackageReference reference in source)
            {
                if (string.IsNullOrEmpty(reference.Id))
                    throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, Local.DeployCommandInvalidPackageReference, new[] { Arguments[1] }));
                if (reference.Version == null)
                    throw new InvalidDataException(string.Format(CultureInfo.CurrentCulture, Local.DeployCommandPackageReferenceInvalidVersion, new[] { reference.Id }));
                list.Add(new DeployMessage.Item
                {
                    PackageId = reference.Id,
                    Version = reference.Version.ToString(),
                });
            }
            return list.ToArray();
        }

        [Option(typeof(Local), "DeployCommandWaitDescription", AltName = "w")]
        public bool Wait { get; set; }

        [Option(typeof(Local), "DeployCommandIncludeDependencyDescription", AltName = "id")]
        public bool IncludeDependency { get; set; }

        [Option(typeof(Local), "DeployCommandApiKey")]
        public string ApiKey { get; set; }

        [Option(typeof(Local), "DeployCommandIncludeVersionDescription", AltName = "iv")]
        public bool IncludeVersion { get; set; }

        public ISettings Settings { get; private set; }

        public IPackageAgentProvider RemoteProvider { get; private set; }

        [Option(typeof(Local), "DeployCommandPrerelease")]
        public bool Prerelease { get; set; }

        [Option(typeof(Local), "DeployCommandVersionDescription")]
        public string Version { get; set; }

        [Option(typeof(Local), "DeployCommandEmailDescription")]
        public string Email { get; set; }

        [Option(typeof(Local), "DeployCommandProjectDescription")]
        public string Project { get; set; }
    }
}
