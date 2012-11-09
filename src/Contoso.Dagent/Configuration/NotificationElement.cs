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
using System.Net.Mail;
namespace Contoso.Configuration
{
    public class NotificationElement : ConfigurationElementEx
    {
        [ConfigurationProperty("fromEmail", IsRequired = true)]
        public string FromEmail
        {
            get { return (string)base["fromEmail"]; }
            set { base["fromEmail"] = value; }
        }

        [ConfigurationProperty("subject", DefaultValue = "Dagent Deploy")]
        public string Subject
        {
            get { return (string)base["subject"]; }
            set { base["subject"] = value; }
        }

        public void SendEmail(string toEmails, string subject, string body)
        {
            string[] addresses;
            if (string.IsNullOrEmpty(FromEmail) || (addresses = (toEmails ?? string.Empty).Split(';')).Length == 0) return;
            // successEmail
            var message = new MailMessage
            {
                IsBodyHtml = false,
                From = new MailAddress(FromEmail),
                Subject = subject,
                Body = body,
            };
            foreach (var address in addresses)
                message.To.Add(new MailAddress(address));
            try { new SmtpClient().Send(message); }
            catch { }
        }
    }
}
