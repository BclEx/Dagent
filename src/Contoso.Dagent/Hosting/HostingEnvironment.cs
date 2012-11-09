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
using System.Security.Permissions;
namespace Contoso.Hosting
{
    public static partial class HostingEnvironment
    {
        internal const int LOGON32_LOGON_INTERACTIVE = 2;
        internal const int LOGON32_PROVIDER_DEFAULT = 0;

        [PermissionSet(SecurityAction.Assert, Unrestricted = true)]
        internal static void Initialize()
        {
            //GetApplicationIdentity();
        }

        internal static IntPtr CreateUserToken(string userName, string password, out string error)
        {
            var domain = "";
            var token = IntPtr.Zero;
            if (UnsafeNativeMethods.LogonUserA(userName, domain, password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, ref token) != 0)
                error = null;
            else
                error = "Unable to login";
            return token;
        }
    }
}
