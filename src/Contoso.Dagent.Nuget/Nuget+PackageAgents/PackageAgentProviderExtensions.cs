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
using System.Collections.Generic;
using System.Linq;
namespace Contoso.NuGet
{
    public static class PackageAgentProviderExtensions
    {
        public static string ResolveAndValidateAgent(this IPackageAgentProvider provider, string agent, out string defaultEmail)
        {
            if (string.IsNullOrEmpty(agent))
            {
                defaultEmail = null;
                return null;
            }
            agent = provider.ResolveAgent(agent, out defaultEmail);
            // Utility.ValidateAgent(agent);
            return agent;
        }

        public static string ResolveAgent(this IPackageAgentProvider provider, string value, out string defaultEmail)
        {
            var resolvedAgent = provider.GetEnabledPackageAgents()
                .Where(remote => remote.Name.Equals(value, StringComparison.CurrentCultureIgnoreCase) || remote.Agent.Equals(value, StringComparison.OrdinalIgnoreCase))
                .Select(remote => remote.Agent).FirstOrDefault();
            defaultEmail = null;
            return (resolvedAgent ?? value);
        }

        public static IEnumerable<PackageAgent> GetEnabledPackageAgents(this IPackageAgentProvider provider)
        {
            return provider.LoadPackageAgents().Where(p => p.IsEnabled);
        }
    }
}
