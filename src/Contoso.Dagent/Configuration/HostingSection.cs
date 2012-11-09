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
using Contoso.Hosting.Deploy;
namespace Contoso.Configuration
{
    public class HostingSection : ConfigurationSectionEx
    {
        [ConfigurationProperty("apiKey")]
        public string ApiKey
        {
            get { return (string)this["apiKey"]; }
            set { this["apiKey"] = value; }
        }

        [ConfigurationProperty("mode", DefaultValue = AgentMode.SingleTenant)]
        public AgentMode Mode
        {
            get { return (AgentMode)this["mode"]; }
            set { this["mode"] = value; }
        }

        [ConfigurationProperty("applications")]
        public ApplicationsElement Applications
        {
            get { return (ApplicationsElement)this["applications"]; }
            set { this["applications"] = value; }
        }

        internal DeployContext MakeDeployContextByID(string apiKey)
        {
            if (apiKey == ApiKey)
                return new DeployContext(null);
            return null;
        }

        public virtual HostingSection Configure(IServiceRegistrar r)
        {
            r.RegisterInstance<HostingSection>(this);
            return this;
        }
    }
}
