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
using System.Configuration;
using System.Abstract;
namespace Contoso.Configuration
{
    public class AppSection : ConfigurationSectionEx
    {
        [ConfigurationProperty("id", IsRequired = true)]
        public string ID
        {
            get { return (string)this["id"]; }
            set { this["id"] = value; }
        }

        [ConfigurationProperty("log4net")]
        public bool Log4Net
        {
            get { return (bool)this["log4net"]; }
            set { this["log4net"] = value; }
        }

        [ConfigurationProperty("notification")]
        public NotificationElement Notification
        {
            get { return (NotificationElement)this["notification"]; }
            set { this["notification"] = value; }
        }

        [ConfigurationProperty("identity")]
        public IdentityElement Identity
        {
            get { return (IdentityElement)this["identity"]; }
            set { this["identity"] = value; }
        }

        public bool HasNotification
        {
            get { return !string.IsNullOrWhiteSpace(Notification.FromEmail); }
        }

        public virtual AppSection Configure(IServiceRegistrar r)
        {
            r.RegisterInstance<AppSection>(this);
            return this;
        }
    }
}
