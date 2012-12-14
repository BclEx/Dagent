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
using System.Runtime.Serialization;
using NuGet;
namespace Contoso.NuGet
{
    [DataContract]
    public class PackageAgent : IEquatable<PackageAgent>
    {
        public PackageAgent(string agent)
            : this(agent, agent, null, true) { }
        public PackageAgent(string agent, string name)
            : this(agent, name, null, true) { }
        public PackageAgent(string agent, string name, string defaultEmail, bool isEnabled)
        {
            if (agent == null)
                throw new ArgumentNullException("agent");
            if (name == null)
                throw new ArgumentNullException("name");
            Name = name;
            Agent = agent;
            DefaultEmail = defaultEmail;
            IsEnabled = isEnabled;
        }

        [DataMember]
        public string Name { get; private set; }

        [DataMember]
        public string Agent { get; private set; }

        [DataMember]
        public string DefaultEmail { get; private set; }

        public bool IsEnabled { get; set; }

        public bool Equals(PackageAgent other)
        {
            if (other == null)
                return false;
            return Name.Equals(other.Name, StringComparison.CurrentCultureIgnoreCase) && Agent.Equals(other.Agent, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            var source = (obj as PackageSource);
            if (obj != null)
                return Equals(source);
            return (obj == null && base.Equals(obj));
        }

        public override string ToString() { return Name + " [" + Agent + "]"; }

        public override int GetHashCode() { return (Name.GetHashCode() * 0xc41) + Agent.GetHashCode(); }

        public PackageAgent Clone() { return new PackageAgent(Agent, Name, DefaultEmail, IsEnabled); }
    }
}
