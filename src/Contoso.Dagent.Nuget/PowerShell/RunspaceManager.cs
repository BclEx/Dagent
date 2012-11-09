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
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using Microsoft.PowerShell;
using PSPowerShell = System.Management.Automation.PowerShell;
namespace Contoso.PowerShell
{
    internal interface IRunspaceManager
    {
        Tuple<Runspace, DagentPSHost> GetRunspace(IConsole console, string hostName);
    }

    internal class RunspaceManager : IRunspaceManager
    {
        private const string PSModulePathEnvVariable = "PSModulePath";
        public const string ProfilePrefix = "Dagent";
        public const string DagentCoreModuleName = "Dagent";

        private readonly ConcurrentDictionary<string, Tuple<Runspace, DagentPSHost>> _runspaceCache = new ConcurrentDictionary<string, Tuple<Runspace, DagentPSHost>>();

        public Tuple<Runspace, DagentPSHost> GetRunspace(IConsole console, string hostName)
        {
            return _runspaceCache.GetOrAdd(hostName, name => CreateAndSetupRunspace(console, name));
        }

        private static Tuple<Runspace, DagentPSHost> CreateAndSetupRunspace(IConsole console, string hostName)
        {
            // set up powershell environment variable for module search path ensuring our own Modules folder is searched before system or user-level 
            AddPowerShellModuleSearchPath();
            var runspace = CreateRunspace(console, hostName);
            LoadModules(runspace.Item1);
            LoadProfilesIntoRunspace(runspace.Item1);
            return runspace;
        }

        private static Tuple<Runspace, DagentPSHost> CreateRunspace(IConsole console, string hostName)
        {
            var initialSessionState = InitialSessionState.CreateDefault();
            var privateData = new Tuple<string, object>[] { };
            var host = new DagentPSHost(hostName, privateData) { ActiveConsole = console };
            var runspace = RunspaceFactory.CreateRunspace(host, initialSessionState);
            runspace.ThreadOptions = PSThreadOptions.Default;
            runspace.Open();
            //
            // Set this runspace as DefaultRunspace so I can script DTE events.
            // WARNING: MSDN says this is unsafe. The runspace must not be shared across threads. I need this to be able to use ScriptBlock for DTE events. The ScriptBlock event handlers execute on DefaultRunspace.
            //
            Runspace.DefaultRunspace = runspace;
            return Tuple.Create(runspace, host);
        }

        private static void AddPowerShellModuleSearchPath()
        {
            var psModulePath = Environment.GetEnvironmentVariable(PSModulePathEnvVariable) ?? String.Empty;
            var extensionRoot = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            // EnvironmentPermission demand?
            Environment.SetEnvironmentVariable(PSModulePathEnvVariable, string.Format(CultureInfo.InvariantCulture, "{0}\\Modules\\;{1}", extensionRoot, psModulePath), EnvironmentVariableTarget.Process);
        }

        private static void LoadModules(Runspace runspace)
        {
            var policy = runspace.GetEffectiveExecutionPolicy();
            if (policy != ExecutionPolicy.Unrestricted && policy != ExecutionPolicy.RemoteSigned && policy != ExecutionPolicy.Bypass)
            {
                var machinePolicy = runspace.GetExecutionPolicy(ExecutionPolicyScope.MachinePolicy);
                var userPolicy = runspace.GetExecutionPolicy(ExecutionPolicyScope.UserPolicy);
                if (machinePolicy == ExecutionPolicy.Undefined && userPolicy == ExecutionPolicy.Undefined)
                    runspace.SetExecutionPolicy(ExecutionPolicy.RemoteSigned, ExecutionPolicyScope.Process);
            }
            runspace.ImportModule(DagentCoreModuleName);
        }

        private static void LoadProfilesIntoRunspace(Runspace runspace)
        {
            using (var powerShell = PSPowerShell.Create())
            {
                powerShell.Runspace = runspace;
                //var profileCommands = HostUtilities.GetProfileCommands(ProfilePrefix);
                //foreach (PSCommand command in profileCommands)
                //{
                //    powerShell.Commands = command;
                //    powerShell.AddCommand("out-default");
                //    powerShell.Invoke();
                //}
            }
        }
    }
}
