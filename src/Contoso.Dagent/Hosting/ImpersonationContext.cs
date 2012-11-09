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
using System.Runtime;
using System.Runtime.InteropServices;
namespace Contoso.Hosting
{
    internal class ImpersonationContext : IDisposable
    {
        private bool _impersonating;
        private bool _reverted;
        private HandleRef _savedToken;

        internal ImpersonationContext() { }
        internal ImpersonationContext(IntPtr token)
        {
            ImpersonateToken(new HandleRef(this, token));
        }

        private void Dispose(bool disposing)
        {
            if (_savedToken.Handle != IntPtr.Zero)
            {
                try { }
                finally { UnsafeNativeMethods.CloseHandle(_savedToken.Handle); _savedToken = new HandleRef(this, IntPtr.Zero); }
            }
        }
        ~ImpersonationContext() { Dispose(false); }

        private static IntPtr GetCurrentToken()
        {
            var zero = IntPtr.Zero;
            if (UnsafeNativeMethods.OpenThreadToken(UnsafeNativeMethods.GetCurrentThread(), 0x2000c, true, ref zero) == 0 && Marshal.GetLastWin32Error() != 0x3f0)
                throw new HostingException("Cannot_impersonate");
            return zero;
        }

        protected void ImpersonateToken(HandleRef token)
        {
            try
            {
                _savedToken = new HandleRef(this, GetCurrentToken());
                if (_savedToken.Handle != IntPtr.Zero && UnsafeNativeMethods.RevertToSelf() != 0)
                    _reverted = true;
                if (token.Handle != IntPtr.Zero)
                {
                    if (UnsafeNativeMethods.SetThreadToken(IntPtr.Zero, token.Handle) == 0)
                        throw new HostingException("Cannot_impersonate");
                    _impersonating = true;
                }
            }
            catch { RestoreImpersonation(); throw; }
        }

        private void RestoreImpersonation()
        {
            if (_impersonating) { UnsafeNativeMethods.RevertToSelf(); _impersonating = false; }
            if (_savedToken.Handle != IntPtr.Zero)
            {
                if (_reverted && UnsafeNativeMethods.SetThreadToken(IntPtr.Zero, _savedToken.Handle) == 0)
                    throw new HostingException("Cannot_impersonate");
                _reverted = false;
            }
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        void IDisposable.Dispose()
        {
            Undo();
        }

        internal void Undo()
        {
            RestoreImpersonation();
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal static bool CurrentThreadTokenExists
        {
            get
            {
                bool flag = false;
                try { }
                finally
                {
                    var currentToken = GetCurrentToken();
                    if (currentToken != IntPtr.Zero)
                    {
                        flag = true;
                        UnsafeNativeMethods.CloseHandle(currentToken);
                    }
                }
                return flag;
            }
        }
    }
}
