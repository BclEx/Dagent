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
using NuGet;
namespace Contoso.Nuget
{
    public interface IPackageAgentProvider
    {
        void DisablePackageAgent(PackageAgent source);
        bool IsPackageAgentEnabled(PackageAgent source);
        IEnumerable<PackageAgent> LoadPackageAgents();
        void SavePackageAgents(IEnumerable<PackageAgent> remotes);
    }

    //[Export(typeof(IPackageAgentProvider))]
    public class PackageAgentProvider : IPackageAgentProvider
    {
        private readonly IEnumerable<PackageAgent> _defaultPackageAgents;
        private readonly IDictionary<PackageAgent, PackageAgent> _migratePackageAgents;
        private readonly ISettings _settingsManager;
        internal const string DisabledPackageAgentsSectionName = "disabledPackageAgents";
        internal const string PackageAgentsSectionName = "packageAgents";

        //[ImportingConstructor]
        public PackageAgentProvider(ISettings settingsManager)
            : this(settingsManager, defaultAgents: null) { }
        public PackageAgentProvider(ISettings settingsManager, IEnumerable<PackageAgent> defaultAgents)
            : this(settingsManager, defaultAgents, migratePackageAgents: null) { }
        public PackageAgentProvider(ISettings settingsManager, IEnumerable<PackageAgent> defaultAgents, IDictionary<PackageAgent, PackageAgent> migratePackageAgents)
        {
            if (settingsManager == null)
                throw new ArgumentNullException("settingsManager");
            _settingsManager = settingsManager;
            _defaultPackageAgents = (defaultAgents ?? Enumerable.Empty<PackageAgent>());
            _migratePackageAgents = migratePackageAgents;
        }

        public void DisablePackageAgent(PackageAgent source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            _settingsManager.SetValue("disabledPackageSources", source.Name, "true");
        }

        public bool IsPackageAgentEnabled(PackageAgent source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            return string.IsNullOrEmpty(_settingsManager.GetValue("disabledPackageSources", source.Name));
        }

        public IEnumerable<PackageAgent> LoadPackageAgents()
        {
            var settingsValues = _settingsManager.GetValues(PackageAgentsSectionName);
            if (settingsValues.IsEmpty())
                return _defaultPackageAgents;
            //
            var disabledAgentsValues = (_settingsManager.GetValues(DisabledPackageAgentsSectionName) ?? Enumerable.Empty<KeyValuePair<string, string>>());
            var disabledAgents = new HashSet<string>(disabledAgentsValues.Select(x => x.Key), StringComparer.CurrentCultureIgnoreCase);
            var loadedPackageAgents = settingsValues.Select(p => new PackageAgent(p.Value, p.Key, null, !disabledAgents.Contains(p.Key))).ToList();
            if (_migratePackageAgents != null)
                MigrateSources(loadedPackageAgents);
            return loadedPackageAgents;
        }

        private void MigrateSources(List<PackageAgent> loadedPackageAgents)
        {
            var hasChanges = false;
            for (var i = 0; i < loadedPackageAgents.Count; i++)
            {
                PackageAgent source;
                var key = loadedPackageAgents[i];
                if (_migratePackageAgents.TryGetValue(key, out source))
                {
                    loadedPackageAgents[i] = source;
                    loadedPackageAgents[i].IsEnabled = key.IsEnabled;
                    hasChanges = true;
                }
            }
            if (hasChanges)
                SavePackageAgents(loadedPackageAgents);
        }

        public void SavePackageAgents(IEnumerable<PackageAgent> remotes)
        {
            _settingsManager.DeleteSection(PackageAgentsSectionName);
            _settingsManager.SetValues(PackageAgentsSectionName, remotes.Select(p => new KeyValuePair<string, string>(p.Name, p.Agent)).ToList());
            _settingsManager.DeleteSection(DisabledPackageAgentsSectionName);
            _settingsManager.SetValues(DisabledPackageAgentsSectionName, remotes.Where(p => !p.IsEnabled).Select(p => new KeyValuePair<string, string>(p.Name, "true")).ToList());
        }
    }
}
