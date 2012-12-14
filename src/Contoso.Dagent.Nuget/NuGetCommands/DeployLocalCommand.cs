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
using System.ComponentModel.Composition;
using System.Reflection;
using Contoso.NuGet;
using NuGet;
using NuGet.Commands;
using NuGet.Common;
namespace Contoso.NuGetCommands
{
    [Command(typeof(Local), "deployLocal", "DeployLocalCommandDescription", UsageDescriptionResourceName = "DeployLocalCommandUsageDescription", UsageSummaryResourceName = "DeployLocalCommandUsageSummary", UsageExampleResourceName = "DeployLocalCommandUsageExamples")]
    public class DeployLocalCommand : InstallCommand
    {
        [ImportingConstructor]
        public DeployLocalCommand(IScriptExecutor scriptExecutor, IPackageRepositoryFactory packageRepositoryFactory, IPackageSourceProvider sourceProvider, IFileSystem startingPoint)
            : base(packageRepositoryFactory, sourceProvider, Settings.LoadDefaultSettings(startingPoint), null)
        {
            ScriptExecutor = scriptExecutor;
            NoCache = true;
        }

        protected override IPackageManager CreatePackageManager(IFileSystem fileSystem)
        {
            var project = Project;
            var packageManager = base_CreatePackageManager(fileSystem);
            EventHandler<PackageOperationEventArgs> installedHandler = (sender, e) =>
            {
                try { ScriptExecutor.ExecuteInstallScript(e.InstallPath, e.Package, project, NullLogger.Instance); }
                catch (Exception ex)
                {
                    Console.WriteLine("InstallScript: " + ex.Message);
                    Console.WriteLine();
                    Console.WriteLine(ex.StackTrace);
                }
            };
            EventHandler<PackageOperationEventArgs> uninstalledHandler = (sender, e) =>
            {
                try { ScriptExecutor.ExecuteUninstallScript(e.InstallPath, e.Package, project, NullLogger.Instance); }
                catch (Exception ex)
                {
                    Console.WriteLine("UninstallScript: " + ex.Message);
                    Console.WriteLine();
                    Console.WriteLine(ex.StackTrace);
                }
            };
            packageManager.PackageInstalled += installedHandler;
            packageManager.PackageUninstalled += uninstalledHandler;
            return packageManager;
        }

        [Option(typeof(Local), "DeployLocalCommandProjectDescription")]
        public object Project { get; set; }

        protected IScriptExecutor ScriptExecutor { get; set; }

        #region base

        private static MethodInfo _getRepositoryInfo = typeof(InstallCommand).GetMethod("GetRepository", BindingFlags.NonPublic | BindingFlags.Instance);

        protected IPackageManager base_CreatePackageManager(IFileSystem fileSystem)
        {
            var sourceRepository = (IPackageRepository)_getRepositoryInfo.Invoke(this, null);
            var allowMultipleVersions = base_AllowMultipleVersions;
            var pathResolver = new DefaultPackagePathResolver(fileSystem, allowMultipleVersions);
            return new PackageManager(sourceRepository, pathResolver, fileSystem, new LocalPackageRepository(pathResolver, fileSystem)) { Logger = Console };
        }

        private bool base_AllowMultipleVersions
        {
            get { return !ExcludeVersion; }
        }

        #endregion
    }
}
