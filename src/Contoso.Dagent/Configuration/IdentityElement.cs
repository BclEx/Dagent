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
using Contoso.Hosting;
namespace Contoso.Configuration
{
    public class IdentityElement : ConfigurationElementEx
    {
        private ImpersonateTokenRef _impersonateTokenRef = new ImpersonateTokenRef(IntPtr.Zero);
        private string _error = string.Empty;
        private string _userName;
        private string _password;

        [ConfigurationProperty("impersonate", DefaultValue = false)]
        public bool Impersonate
        {
            get { return (bool)base["impersonate"]; }
            set { base["impersonate"] = value; }
        }

        [ConfigurationProperty("password", DefaultValue = "")]
        public string Password
        {
            get { return (string)base["password"]; }
            set { base["password"] = value; }
        }

        [ConfigurationProperty("userName", DefaultValue = "")]
        public string UserName
        {
            get { return (string)base["userName"]; }
            set { base["userName"] = value; }
        }

        internal IntPtr ImpersonateToken
        {
            get
            {
                if (_impersonateTokenRef.Handle == IntPtr.Zero && !string.IsNullOrEmpty(_userName) && Impersonate)
                    InitializeToken();
                return _impersonateTokenRef.Handle;
            }
        }

        private void InitializeToken()
        {
            _error = string.Empty;
            var token = HostingEnvironment.CreateUserToken(_userName, _password, out _error);
            _impersonateTokenRef = new ImpersonateTokenRef(token);
            if (_impersonateTokenRef.Handle == IntPtr.Zero)
            {
                if (_error.Length > 0)
                    throw new ConfigurationErrorsException(string.Format("Invalid_credentials_2", _error), ElementInformation.Properties["userName"].Source, ElementInformation.Properties["userName"].LineNumber);
                throw new ConfigurationErrorsException("Invalid_credentials", base.ElementInformation.Properties["userName"].Source, base.ElementInformation.Properties["userName"].LineNumber);
            }
        }

        public IdentityElement Configure()
        {
            var _userName = UserName;
            var _password = Password;
            //if (!ConfigurationManagerEx.CheckAndReadRegistryValue(ref _userName, false))
            //    throw new ConfigurationErrorsException("Invalid_registry_config", ElementInformation.Source, ElementInformation.LineNumber);
            //if (!ConfigurationManagerEx.CheckAndReadRegistryValue(ref _password, false))
            //    throw new ConfigurationErrorsException("Invalid_registry_config", ElementInformation.Source, ElementInformation.LineNumber);
            if (_userName != null && _userName.Length < 1)
                _userName = null;
            if (_userName != null && Impersonate)
            {
                if (_password == null)
                    _password = string.Empty;
            }
            else if (_password != null && _userName == null && _password.Length > 0 && Impersonate)
                throw new ConfigurationErrorsException("Invalid_credentials", ElementInformation.Properties["password"].Source, ElementInformation.Properties["password"].LineNumber);
            if (Impersonate && ImpersonateToken == IntPtr.Zero && _userName != null)
            {
                if (_error.Length > 0)
                    throw new ConfigurationErrorsException(string.Format("Invalid_credentials_2", _error), ElementInformation.Properties["userName"].Source, ElementInformation.Properties["userName"].LineNumber);
                throw new ConfigurationErrorsException("Invalid_credentials", ElementInformation.Properties["userName"].Source, ElementInformation.Properties["userName"].LineNumber);
            }
            return this;
        }
    }
}
