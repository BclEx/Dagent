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
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Security;
using SysConsole = System.Console;
namespace Contoso.PowerShell
{
    internal class DagentPSHostUserInterface : PSHostUserInterface
    {
        public const ConsoleColor NoColor = (ConsoleColor)(-1);
        private const int VkCodeReturn = 13;
        private const int VkCodeBackspace = 8;
        private readonly DagentPSHost _host;
        private readonly object _lock = new object();
        private PSHostRawUserInterface _rawUI;

        public DagentPSHostUserInterface(DagentPSHost host)
        {
            if (host == null)
                throw new ArgumentNullException("host");
            _host = host;
        }

        private IConsole Console
        {
            get { return _host.ActiveConsole; }
        }

        public override PSHostRawUserInterface RawUI
        {
            get
            {
                if (_rawUI == null)
                    _rawUI = new DagentPSHostRawUserInterface(_host);
                return _rawUI;
            }
        }

        private static Type GetFieldType(FieldDescription field)
        {
            Type type = null;
            if (!string.IsNullOrEmpty(field.ParameterAssemblyFullName))
                LanguagePrimitives.TryConvertTo(field.ParameterAssemblyFullName, out type);
            if (type == null && !string.IsNullOrEmpty(field.ParameterTypeFullName))
                LanguagePrimitives.TryConvertTo(field.ParameterTypeFullName, out type);
            return type;
        }

        public override Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions)
        {
            if (descriptions == null)
                throw new ArgumentNullException("descriptions");
            // emulate powershell.exe behavior for empty collection.
            if (descriptions.Count == 0)
                throw new ArgumentException("Local.ZeroLengthCollection", "descriptions");
            if (!string.IsNullOrEmpty(caption))
                WriteLine(caption);
            if (!string.IsNullOrEmpty(message))
                WriteLine(message);
            // this stores the field/value pairs - e.g. unbound missing mandatory parameters, or scripted $host.ui.prompt invocation.
            var results = new Dictionary<string, PSObject>(descriptions.Count);
            var index = 0;
            foreach (FieldDescription description in descriptions)
            {
                // if type is not resolvable, throw (as per powershell.exe)
                if (description == null || string.IsNullOrEmpty(description.ParameterAssemblyFullName))
                    throw new ArgumentException("descriptions[" + index + "]");
                bool cancelled;
                object answer;
                var name = description.Name;
                // as per powershell.exe, if input value cannot be coerced to parameter type then default to string.
                var fieldType = (GetFieldType(description) ?? typeof(String));
                //// is parameter a collection type?
                //if (typeof(IList).IsAssignableFrom(fieldType))
                //    // [int[]]$param1, [string[]]$param2
                //    cancelled = PromptCollection(name, fieldType, out answer);
                //else
                //    // [int]$param1, [string]$param2
                //    cancelled = PromptScalar(name, fieldType, out answer);
                cancelled = PromptNothing(name, fieldType, out answer);
                // user hit ESC?
                if (cancelled)
                {
                    WriteLine(string.Empty);
                    results.Clear();
                    break;
                }
                results.Add(name, PSObject.AsPSObject(answer));
                index++;
            }
            return results;
        }

        private static bool PromptNothing(string name, Type fieldType, out object answer)
        {
            answer = null;
            return true;
        }

        public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
        {
            if (!string.IsNullOrEmpty(caption))
                WriteLine(caption);
            if (!string.IsNullOrEmpty(message))
                WriteLine(message);
            var chosen = -1;
            do
            {
                // holds hotkeys, e.g. "[Y] Yes [N] No"
                var accelerators = new string[choices.Count];
                for (var index = 0; index < choices.Count; index++)
                {
                    var choice = choices[index];
                    var label = choice.Label;
                    var ampIndex = label.IndexOf('&'); // hotkey marker
                    accelerators[index] = string.Empty; // default to empty
                    // accelerator marker found?
                    if (ampIndex != -1 && ampIndex < (label.Length - 1))
                        // grab the letter after '&'
                        accelerators[index] = label.Substring(ampIndex + 1, 1).ToUpper(CultureInfo.CurrentCulture);
                    // remove the redundant marker from output
                    Write(string.Format(CultureInfo.CurrentCulture, "[{0}] {1}  ", accelerators[index], label.Replace("&", string.Empty)));
                }
                Write(string.Format(CultureInfo.CurrentCulture, "Local.PromptForChoiceSuffix", accelerators[defaultChoice]));
                var input = ReadLine();
                switch (input.Length)
                {
                    // enter, accept default if provided
                    case 0:
                        if (defaultChoice == -1)
                            continue;
                        chosen = defaultChoice;
                        break;
                    // single letter accelerator, e.g. "Y"
                    case 1: chosen = Array.FindIndex(accelerators, accelerator => accelerator.Equals(input, StringComparison.OrdinalIgnoreCase)); break;
                    // match against entire label, e.g. "Yes"
                    default: chosen = Array.FindIndex(choices.ToArray(), choice => choice.Label.Equals(input, StringComparison.OrdinalIgnoreCase)); break;
                }
            } while (chosen == -1);
            return chosen;
        }

        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName) { return PromptForCredential(caption, message, userName, targetName, PSCredentialTypes.Default, PSCredentialUIOptions.Default); }
        public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
        {
            return null;
            //return NativeMethods.CredUIPromptForCredentials(caption, message, userName, targetName, allowedCredentialTypes, options);
        }

        public override string ReadLine()
        {
            return null;
            //try
            //{
            //    var b = new StringBuilder();
            //    lock (_lock)
            //    {
            //        KeyInfo keyInfo;
            //        while ((keyInfo = RawUI.ReadKey()).VirtualKeyCode != VkCodeReturn)
            //        {
            //            // {enter}
            //            if (keyInfo.VirtualKeyCode == VkCodeBackspace)
            //            {
            //                if (b.Length > 0)
            //                {
            //                    b.Remove(b.Length - 1, 1);
            //                    Console.WriteBackspace();
            //                }
            //            }
            //            else
            //            {
            //                b.Append(keyInfo.Character);
            //                // destined for output, so apply culture
            //                Write(keyInfo.Character.ToString(CultureInfo.CurrentCulture));
            //            }
            //        }
            //    }
            //    return b.ToString();
            //}
            //// ESC was hit
            //catch (PipelineStoppedException) { return null; }
            //finally { WriteLine(string.Empty); }
        }

        public override SecureString ReadLineAsSecureString()
        {
            return null;
            //var secureString = new SecureString();
            //try
            //{
            //    lock (_lock)
            //    {
            //        KeyInfo keyInfo;
            //        while ((keyInfo = RawUI.ReadKey()).VirtualKeyCode != VkCodeReturn)
            //        {
            //            // {enter}
            //            if (keyInfo.VirtualKeyCode == VkCodeBackspace)
            //            {
            //                if (secureString.Length > 0)
            //                {
            //                    secureString.RemoveAt(secureString.Length - 1);
            //                    Console.WriteBackspace();
            //                }
            //            }
            //            else
            //            {
            //                // culture is deferred until securestring is decrypted
            //                secureString.AppendChar(keyInfo.Character);
            //                Write("*");
            //            }
            //        }
            //        secureString.MakeReadOnly();
            //    }
            //    return secureString;
            //}
            //// ESC was hit, clean up secure string
            //catch (PipelineStoppedException) { secureString.Dispose(); return null; }
            //finally { WriteLine(string.Empty); }
        }

        public override void Write(string value) { SysConsole.Write(value); }
        public override void WriteLine(string value) { SysConsole.WriteLine(value); }
        public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value) { Write(value, foregroundColor, backgroundColor); }
        public override void WriteDebugLine(string message) { WriteLine(message, ConsoleColor.DarkGray); }
        public override void WriteErrorLine(string value) { WriteLine(value, ConsoleColor.Red); }
        public override void WriteProgress(long sourceId, ProgressRecord record)
        {
            //var operation = (record.CurrentOperation ?? record.StatusDescription);
            //if (!string.IsNullOrEmpty(operation))
            //    Console.WriteProgress(operation, record.PercentComplete);
        }

        public override void WriteVerboseLine(string message) { WriteLine(message, ConsoleColor.DarkGray); }
        public override void WriteWarningLine(string message) { WriteLine(message, ConsoleColor.Magenta); }

        private void WriteLine(string value, ConsoleColor foregroundColor, ConsoleColor backgroundColor = NoColor) { Write(value + Environment.NewLine, foregroundColor, backgroundColor); }
        private void Write(string value, ConsoleColor foregroundColor, ConsoleColor backgroundColor = NoColor) { SysConsole.Write(value); }
    }
}
