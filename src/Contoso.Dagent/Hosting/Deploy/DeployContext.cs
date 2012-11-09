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
namespace Contoso.Hosting.Deploy
{
    public class DeployContext : IDisposable
    {
        private ImpersonationContext _impersonationContext;
        private DeployApplication _app;

        public DeployContext(DeployApplication app)
        {
            _app = app;
            if (HostingEnvironment.ImpersonationEnabled)
                SetImpersonationContext();
        }

        public void Dispose()
        {
            UndoImpersonationContext();
        }

        internal IntPtr ImpersonationToken
        {
            get
            {
                var identity = HostingEnvironment.Identity;
                if (identity != null && identity.Impersonate)
                    return (identity.ImpersonateToken != IntPtr.Zero ? identity.ImpersonateToken : ClientIdentityToken);
                return HostingEnvironment.ApplicationIdentityToken;
            }
        }

        internal IntPtr ClientIdentityToken
        {
            get
            {
                if (_app != null)
                    return _app.GetUserToken();
                return IntPtr.Zero;
            }
        }

        internal void SetImpersonationContext()
        {
            if (_impersonationContext == null)
                _impersonationContext = new ClientImpersonationContext(this);
        }

        internal void UndoImpersonationContext()
        {
            if (_impersonationContext != null) { _impersonationContext.Undo(); _impersonationContext = null; }
        }
    }
}
