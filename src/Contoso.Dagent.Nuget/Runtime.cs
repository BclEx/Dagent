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
using System.Reflection;
using Contoso.Abstract;
using Rhino.ServiceBus.Config;
using System.Diagnostics;
namespace Contoso
{
    #region Runtime-Hook

    internal class Runtime
    {
        private static readonly string DagentBinPath = GetDagentBinPath();

        static Runtime()
        {
            if (DagentBinPath == null)
                throw new InvalidOperationException("Dagent not located.");
            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += (sender, args) =>
            {
                var assemblyPath = Path.Combine(DagentBinPath, new AssemblyName(args.Name).Name + ".dll");
                if (!File.Exists(assemblyPath)) return null;
                return Assembly.LoadFrom(assemblyPath);
            };
            RuntimeBase.Ensure();
        }

        internal static void Ensure() { }

        private static string GetDagentBinPath()
        {
            var dagentPath = Environment.GetEnvironmentVariable("DagentPath");
            if (dagentPath == null) return null;
            return Path.Combine(dagentPath, "bin\\");
        }
    }

    #endregion

    internal class RuntimeBase
    {
        static RuntimeBase()
        {
            var busConfiguration = new BusConfigurationSection();
            busConfiguration.Bus.Endpoint = "msmq://localhost/Dagent.nuget";
            ServiceBusManager.SetProvider(() => new RhinoServiceBusAbstractor(busConfiguration));
        }

        internal static void Ensure() { }
    }
}
