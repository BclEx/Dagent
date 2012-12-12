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
using System.Collections.ObjectModel;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Threading;
using Microsoft.PowerShell;
namespace Contoso.PowerShell
{
    internal static class RunspaceExtensions
    {
        public static Collection<PSObject> Invoke(this Runspace runspace, string command, object[] inputs, bool outputResults)
        {
            if (string.IsNullOrEmpty(command))
                throw new ArgumentNullException("command");
            using (Pipeline pipeline = CreatePipeline(runspace, command, outputResults))
                return (inputs != null ? pipeline.Invoke(inputs) : pipeline.Invoke());
        }

        public static Collection<PSObject> Invoke(this Runspace runspace, string command, object[] inputs, bool outputResults, int timeoutMilliseconds)
        {
            if (string.IsNullOrEmpty(command))
                throw new ArgumentNullException("command");
            using (Pipeline pipeline = CreatePipeline(runspace, command, outputResults))
            {
                Thread threadToKill = null;
                Func<Collection<PSObject>> action = () =>
                {
                    threadToKill = Thread.CurrentThread;
                    return (inputs != null ? pipeline.Invoke(inputs) : pipeline.Invoke());
                };
                var result = action.BeginInvoke(null, null);
                if (result.AsyncWaitHandle.WaitOne(timeoutMilliseconds))
                    return action.EndInvoke(result);
                threadToKill.Abort();
                throw new TimeoutException();
            }
        }

        public static Pipeline InvokeAsync(this Runspace runspace, string command, object[] inputs, bool outputResults, EventHandler<PipelineStateEventArgs> pipelineStateChanged)
        {
            if (string.IsNullOrEmpty(command))
                throw new ArgumentNullException("command");
            var pipeline = CreatePipeline(runspace, command, outputResults);
            pipeline.StateChanged += (sender, e) =>
            {
                var ev = pipelineStateChanged;
                if (ev != null)
                    ev(sender, e);
                // Dispose Pipeline object upon completion
                switch (e.PipelineStateInfo.State)
                {
                    case PipelineState.Completed:
                    case PipelineState.Failed:
                    case PipelineState.Stopped:
                        ((Pipeline)sender).Dispose();
                        break;
                }
            };
            if (inputs != null)
                foreach (var input in inputs)
                    pipeline.Input.Write(input);
            pipeline.InvokeAsync();
            return pipeline;
        }

        public static string ExtractErrorFromErrorRecord(this Runspace runspace, ErrorRecord record)
        {
            var pipeline = runspace.CreatePipeline("$input", false);
            pipeline.Commands.Add("out-string");
            Collection<PSObject> result;
            using (PSDataCollection<object> inputCollection = new PSDataCollection<object>())
            {
                inputCollection.Add(record);
                inputCollection.Complete();
                result = pipeline.Invoke(inputCollection);
            }
            if (result.Count > 0)
            {
                var str = (result[0].BaseObject as string);
                if (!string.IsNullOrEmpty(str))
                    // Remove \r\n, which is added by the Out-String cmdlet.
                    return str.Substring(0, str.Length - 2);
            }
            return String.Empty;
        }

        private static Pipeline CreatePipeline(Runspace runspace, string command, bool outputResults)
        {
            var pipeline = runspace.CreatePipeline(command, addToHistory: true);
            if (outputResults)
            {
                pipeline.Commands.Add("out-default");
                pipeline.Commands[0].MergeMyResults(PipelineResultTypes.Error, PipelineResultTypes.Output);
            }
            return pipeline;
        }

        public static ExecutionPolicy GetEffectiveExecutionPolicy(this Runspace runspace) { return GetExecutionPolicy(runspace, "Get-ExecutionPolicy"); }

        public static ExecutionPolicy GetExecutionPolicy(this Runspace runspace, ExecutionPolicyScope scope) { return GetExecutionPolicy(runspace, "Get-ExecutionPolicy -Scope " + scope); }

        private static ExecutionPolicy GetExecutionPolicy(this Runspace runspace, string command)
        {
            var results = runspace.Invoke(command, null, false);
            return (results.Count > 0 ? (ExecutionPolicy)results[0].BaseObject : ExecutionPolicy.Undefined);
        }

        public static void SetExecutionPolicy(this Runspace runspace, ExecutionPolicy policy, ExecutionPolicyScope scope)
        {
            var command = string.Format(CultureInfo.InvariantCulture, "Set-ExecutionPolicy {0} -Scope {1} -Force", policy.ToString(), scope.ToString());
            runspace.Invoke(command, null, false);
        }

        public static void ImportModule(this Runspace runspace, string modulePath) { runspace.Invoke("Import-Module " + Utility.EscapePSPath(modulePath), null, false); }

        public static void ChangePSDirectory(this Runspace runspace, string directory)
        {
            if (!string.IsNullOrWhiteSpace(directory))
                runspace.Invoke("Set-Location " + Utility.EscapePSPath(directory), null, false);
        }
    }
}
