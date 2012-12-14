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
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;
using NuGet;
using Contoso.PowerShell;
using MyPowerShell = System.Management.Automation.PowerShell;
using System.Reflection;
namespace Contoso.NuGet
{
    public interface IScriptExecutor
    {
        void ExecuteInstallScript(string installPath, IPackage package, object project, ILogger logger);
        void ExecuteUninstallScript(string installPath, IPackage package, object project, ILogger logger);
    }

    [Export(typeof(IScriptExecutor))]
    public class PSScriptExecutor : IScriptExecutor
    {
        private static object _lock = new object();

        public void ExecuteInstallScript(string installPath, IPackage package, object project, ILogger logger)
        {
            lock (_lock)
                try
                {
                    var fullPath = Path.Combine(installPath, "tools\\install.cmd");
                    if (File.Exists(fullPath))
                    {
                        logger.Log(MessageLevel.Info, string.Format(CultureInfo.CurrentCulture, "ExecutingScript {0}", fullPath));
                        var toolsPath = Path.GetDirectoryName(fullPath);
                        Environment.CurrentDirectory = toolsPath;
                        RunCMD(fullPath, new object[] { installPath, toolsPath, package.Id, project });
                        return;
                    }
                    fullPath = Path.Combine(installPath, "tools\\install.ps1");
                    if (File.Exists(fullPath))
                    {
                        logger.Log(MessageLevel.Info, string.Format(CultureInfo.CurrentCulture, "ExecutingScript {0}", fullPath));
                        var toolsPath = Path.GetDirectoryName(fullPath);
                        Environment.CurrentDirectory = toolsPath;
                        RunPS1(fullPath, new object[] { installPath, toolsPath, package, project });
                        return;
                    }
                }
                finally { Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location); }
        }

        public void ExecuteUninstallScript(string installPath, IPackage package, object project, ILogger logger)
        {
            lock (_lock)
                try
                {
                    var fullPath = Path.Combine(installPath, "tools\\uninstall.cmd");
                    if (File.Exists(fullPath))
                    {
                        logger.Log(MessageLevel.Info, string.Format(CultureInfo.CurrentCulture, "ExecutingScript {0}", fullPath));
                        var toolsPath = Path.GetDirectoryName(fullPath);
                        Environment.CurrentDirectory = toolsPath;
                        RunCMD(fullPath, new object[] { installPath, toolsPath, package.Id, project });
                        return;
                    }
                    fullPath = Path.Combine(installPath, "tools\\uninstall.ps1");
                    if (File.Exists(fullPath))
                    {
                        logger.Log(MessageLevel.Info, string.Format(CultureInfo.CurrentCulture, "ExecutingScript {0}", fullPath));
                        var toolsPath = Path.GetDirectoryName(fullPath);
                        Environment.CurrentDirectory = toolsPath;
                        RunPS1(fullPath, new object[] { installPath, toolsPath, package, project });
                        return;
                    }
                }
                finally { Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location); }
        }

        private void RunCMD(string fullPath, object[] args)
        {
            var cmd = string.Format("/C \"{0}\" \"{1}\" \"{2}\" \"{3}\"", fullPath, args[0], args[1], args[2]);
            RunProcess("cmd.exe", cmd, 5 * 60 * 1000);
        }

        private void RunPS1(string fullPath, object[] args)
        {
            var cmd = string.Format("-NonInteractive -ExecutionPolicy Unrestricted -File \"{0}\" \"{1}\" \"{2}\" \"{3}\"", fullPath, args[0], args[1], args[2]);
            Console.WriteLine("powershell.exe " + cmd);
            RunProcess("powershell.exe", cmd, 1 * 60 * 1000);
        }

        private void RunPS1_(string fullPath, object[] args)
        {
            var cmd = "$__pc_args=@(); $input|%{$__pc_args+=$_}; & " + Utility.EscapePSPath(fullPath) + " $__pc_args[0] $__pc_args[1] $__pc_args[2] $__pc_args[3]; Remove-Variable __pc_args -Scope 0";
            IEnumerable<PSObject> results;
            using (var runspace = RunspaceFactory.CreateRunspace())
            {
                runspace.Open();
                results = runspace.Invoke(cmd, args, true, 5 * 60 * 1000);
                runspace.Close();
            }
            if (results != null)
                foreach (PSObject obj in results)
                    Console.WriteLine(obj.ToString());
        }

        private void RunProcess(string fileName, string arguments, int timeoutMilliseconds)
        {
            Thread threadToKill = null;
            Action action = () =>
            {
                threadToKill = Thread.CurrentThread;
                using (var p = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = fileName,
                            Arguments = arguments,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                        }
                    })
                {
                    p.Start();
                    Console.WriteLine(p.StandardOutput.ReadToEnd());
                    p.WaitForExit();
                    var errors = p.StandardError.ReadToEnd();
                    if (!string.IsNullOrEmpty(errors))
                        Console.WriteLine(errors);
                }
            };
            var result = action.BeginInvoke(null, null);
            if (result.AsyncWaitHandle.WaitOne(timeoutMilliseconds))
            {
                action.EndInvoke(result);
                return;
            }
            threadToKill.Abort();
            throw new TimeoutException();
        }
    }
}
