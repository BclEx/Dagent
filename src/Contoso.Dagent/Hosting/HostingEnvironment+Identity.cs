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
using System.Configuration;
using System.Security.Permissions;
using Contoso.Configuration;
namespace Contoso.Hosting
{
    public partial class HostingEnvironment
    {
        internal static readonly IdentityElement Identity = ConfigurationManagerEx.GetSection<AppSection>("appSection").Identity.Configure();
        internal static readonly bool ImpersonationEnabled = (Identity != null && Identity.Impersonate);
        private static readonly IntPtr _defaultIdentityToken = IntPtr.Zero;
        private static IntPtr _applicationIdentityToken;

        private static void GetApplicationIdentity()
        {
            try { _applicationIdentityToken = (Identity.Impersonate && Identity.ImpersonateToken != IntPtr.Zero ? Identity.ImpersonateToken : _defaultIdentityToken); }
            catch { }
        }

        [SecurityPermission(SecurityAction.Demand, ControlPrincipal = true)]
        public static IDisposable Impersonate() { return new ApplicationImpersonationContext(); }
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public static IDisposable Impersonate(IntPtr token) { return (token == IntPtr.Zero ? new ProcessImpersonationContext() : new ImpersonationContext(token)); }

        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        public static IDisposable ImpersonateUser(IntPtr userToken)
        {
            if (!Identity.Impersonate)
                return new ApplicationImpersonationContext();
            if (Identity.ImpersonateToken != IntPtr.Zero)
                return new ImpersonationContext(Identity.ImpersonateToken);
            return new ImpersonationContext(userToken);
        }

        internal static IntPtr ApplicationIdentityToken
        {
            get { return _applicationIdentityToken; }
        }
    }
}
