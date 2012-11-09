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
using System.IO;
using System.Linq;
using System.Text;
using Contoso.Bus.NugetMessages;
using Contoso.Configuration;
using Contoso.NuGetCommands;
using NuGet;
using Contoso.Hosting;
namespace Contoso.Bus.NugetHandlers
{
    public class DeployHandler : Rhino.ServiceBus.ConsumerOf<DeployMessage>
    {
        private static readonly SemanticVersion _emptySemanticVersion = SemanticVersion.Parse("0.0.0.1");
        public IServiceBus _bus;
        public AppSection _appSection;
        public HostingSection _hostingSection;

        public DeployHandler(IServiceBus bus, AppSection appSection, HostingSection hostingSection)
        {
            _bus = bus;
            _appSection = appSection;
            _hostingSection = hostingSection;
        }

        public void Consume(DeployMessage message)
        {
            var outputDirectory = (Environment.GetEnvironmentVariable("DagentPackagesPath") ?? @"C:\Program Files\Dagent\Packages\");
            if (Directory.Exists(outputDirectory))
                Directory.Delete(outputDirectory, true);
            Directory.CreateDirectory(outputDirectory);
            //
            string body;
            using (var context = _hostingSection.MakeDeployContextByID(message.ApiKey))
                if (context != null)
                {
                    var b = new StringBuilder();
                    string packagesDirectory = null;
                    try
                    {
                        string arg0;
                        string version;
                        if (message.FromSpec)
                            InstallPackagesFromConfigFile(message.Items, out packagesDirectory, out arg0, out version);
                        else
                            InstallPackagesInline(message.Items, out arg0, out version);
                        // install
                        NuGet.NuMain(b, null, m =>
                            {
                                var command = (DeployLocalCommand)m.GetCommands().FirstOrDefault(x => x is DeployLocalCommand);
                                if (command == null)
                                    throw new NullReferenceException("Unable to find DeployInstallCommand");
                                command.Prerelease = message.Prerelease;
                                command.ExcludeVersion = message.ExcludeVersion;
                                command.Version = version;
                                command.Source.Add("http://nuget.degdarwin.com/nuget");
                                //command.Source.Add("http://nuget.degdarwin.com/Dagent/nuget");
                                command.OutputDirectory = outputDirectory;
                                command.Project = message.Project;
                                command.Arguments.Add(arg0);
                                return command;
                            });
                    }
                    catch (Exception ex)
                    {
                        b.AppendLine(ex.Message);
                        b.AppendLine();
                        b.AppendLine(ex.StackTrace);
                    }
                    finally { if (packagesDirectory != null && Directory.Exists(packagesDirectory)) Directory.Delete(packagesDirectory, true); }
                    body = b.ToString();
                }
                else
                    body = "Incorrect Application Key " + message.ApiKey;
            // notification
            var notification = _appSection.Notification;
            if (_appSection.HasNotification && notification != null && !string.IsNullOrEmpty(message.Email))
                notification.SendEmail(message.Email, string.Format(notification.Subject, message.Project), body);
        }

        public void InstallPackagesInline(DeployMessage.Item[] items, out string packageId, out string version)
        {
            if (items == null || items.Length != 1)
                throw new InvalidOperationException();
            var item = items[0];
            packageId = item.PackageId;
            version = item.Version;
        }

        public void InstallPackagesFromConfigFile(DeployMessage.Item[] items, out string packagesDirectory, out string packagesFile, out string version)
        {
            version = null;
            packagesFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + "\\packages.config");
            packagesDirectory = Path.GetDirectoryName(packagesFile);
            if (!Directory.Exists(packagesDirectory)) Directory.CreateDirectory(packagesDirectory);
            var referenceFile = new PackageReferenceFile(packagesFile);
            foreach (var item in items)
                referenceFile.AddEntry(item.PackageId, (SemanticVersion.ParseOptionalVersion(item.Version) ?? _emptySemanticVersion));
        }
    }
}