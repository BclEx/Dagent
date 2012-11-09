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
using System.Management.Automation.Host;
namespace Contoso.PowerShell
{
    internal class DagentPSHostRawUserInterface : PSHostRawUserInterface
    {
        private const int Console_ConsoleWidth = 256;
        private readonly DagentPSHost _host;

        public DagentPSHostRawUserInterface(DagentPSHost host)
        {
            _host = host;
        }

        private IConsole Console
        {
            get { return _host.ActiveConsole; }
        }

        public override ConsoleColor BackgroundColor
        {
            get { return DagentPSHostUserInterface.NoColor; }
            set { }
        }

        public override Size BufferSize
        {
            get { return new Size(Console_ConsoleWidth, 0); }
            set { throw new NotImplementedException(); }
        }

        public override Coordinates CursorPosition
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public override int CursorSize
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public override void FlushInputBuffer() { throw new NotImplementedException(); }

        public override ConsoleColor ForegroundColor
        {
            get { return DagentPSHostUserInterface.NoColor; }
            set { }
        }

        public override BufferCell[,] GetBufferContents(Rectangle rectangle) { throw new NotImplementedException(); }

        public override bool KeyAvailable
        {
            //get { return Console.Dispatcher.IsKeyAvailable; }
            get { return false; }
        }

        public override Size MaxPhysicalWindowSize
        {
            get { throw new NotImplementedException(); }
        }

        public override Size MaxWindowSize
        {
            get { throw new NotImplementedException(); }
        }

        public override KeyInfo ReadKey(ReadKeyOptions options)
        {
            return new KeyInfo();
            //// NOTE: readkey options are ignored as they are not really usable or applicable in PM console.
            //var keyInfo = Console.Dispatcher.WaitKey();
            //if (keyInfo == null)
            //    // abort current pipeline (ESC pressed)
            //    throw new PipelineStoppedException();
            //var states = default(ControlKeyStates);
            //states |= (keyInfo.CapsLockToggled ? ControlKeyStates.CapsLockOn : 0);
            //states |= (keyInfo.NumLockToggled ? ControlKeyStates.NumLockOn : 0);
            //states |= (keyInfo.ShiftPressed ? ControlKeyStates.ShiftPressed : 0);
            //states |= (keyInfo.AltPressed ? ControlKeyStates.LeftAltPressed : 0); // assume LEFT alt
            //states |= (keyInfo.ControlPressed ? ControlKeyStates.LeftCtrlPressed : 0); // assume LEFT ctrl
            //return new KeyInfo(keyInfo.VirtualKey, keyInfo.KeyChar, states, keyDown: (keyInfo.KeyStates == KeyStates.Down));
        }

        public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, BufferCell fill) { throw new NotImplementedException(); }

        public override void SetBufferContents(Rectangle rectangle, BufferCell fill) { throw new NotImplementedException(); }

        public override void SetBufferContents(Coordinates origin, BufferCell[,] contents) { throw new NotImplementedException(); }

        public override Coordinates WindowPosition
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public override Size WindowSize
        {
            get { return new Size(Console_ConsoleWidth, 0); }
            set { throw new NotImplementedException(); }
        }

        public override string WindowTitle
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }
    }
}
