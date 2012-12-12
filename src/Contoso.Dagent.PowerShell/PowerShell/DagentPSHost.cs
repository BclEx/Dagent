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
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Threading;
namespace Contoso.PowerShell
{
    internal class DagentPSHost : PSHost
    {
        private readonly CultureInfo _culture = Thread.CurrentThread.CurrentCulture;
        private readonly Guid _instanceId = Guid.NewGuid();
        private readonly string _name;
        private readonly PSObject _privateData;
        private readonly CultureInfo _uiCulture = Thread.CurrentThread.CurrentUICulture;
        private PSHostUserInterface _ui;

        private class Commander
        {
            private readonly DagentPSHost _host;

            public Commander(DagentPSHost host)
            {
                _host = host;
            }

            public void ClearHost()
            {
                if (_host.ActiveConsole != null)
                    _host.ActiveConsole.Clear();
            }
        }

        public DagentPSHost(string name, params Tuple<string, object>[] extraData)
        {
            _name = name;
            _privateData = new PSObject(new Commander(this));
            foreach (Tuple<string, object> tuple in extraData)
                _privateData.Properties.Add(new PSNoteProperty(tuple.Item1, tuple.Item2));
        }

        public IConsole ActiveConsole { get; set; }

        public override CultureInfo CurrentCulture
        {
            get { return _culture; }
        }

        public override CultureInfo CurrentUICulture
        {
            get { return _uiCulture; }
        }

        public override Guid InstanceId
        {
            get { return _instanceId; }
        }

        public override string Name
        {
            get { return _name; }
        }

        public override PSObject PrivateData
        {
            get { return _privateData; }
        }

        public override PSHostUserInterface UI
        {
            get
            {
                if (_ui == null)
                    _ui = new DagentPSHostUserInterface(this);
                return _ui;
            }
        }

        public override Version Version
        {
            get { return GetType().Assembly.GetName().Version; }
        }

        public override void EnterNestedPrompt() { throw new NotSupportedException(); } // UI.WriteErrorLine("ErrorNestedPromptNotSupported"); }

        public override void ExitNestedPrompt() { throw new NotSupportedException(); }

        public override void NotifyBeginApplication() { }

        public override void NotifyEndApplication() { }

        public override void SetShouldExit(int exitCode) { }
    }
}
