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
using NuGet;
using System;
namespace Contoso
{
    internal class Utility
    {
        public static readonly string ApiKeysSectionName = "apikeys";

        public static string GetRemoteApiKey(ISettings settings, string remote, bool throwIfNotFound = true)
        {
            var str = settings.GetDecryptedValue(ApiKeysSectionName, remote);
            if (string.IsNullOrEmpty(str) && throwIfNotFound)
                throw new CommandLineException(Local.NoAgentApiKeyFound, new object[] { GetRemoteDisplayName(remote) });
            return str;
        }

        public static string GetRemoteDisplayName(string remote) { return ("'" + remote + "'"); }

        public static string EscapePSPath(string path)
        {
            if (path == null)
                throw new ArgumentNullException("path");
            path = path.Replace("[", "`[").Replace("]", "`]");
            return (path.Contains("'") ? "\"" + path.Replace("$", "`$") + "\"" : "'" + path + "'");
        }

        public static bool IsValidAgent(string agent) { return PathValidator.IsValidUrl(agent); }
    }
}
