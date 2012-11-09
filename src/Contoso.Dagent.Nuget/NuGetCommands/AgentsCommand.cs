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
using System.ComponentModel.Composition;
using System.Linq;
using NuGet;
using NuGet.Commands;
using NuGet.Common;
using Contoso.Nuget;
namespace Contoso.NuGetCommands
{
    [Command(typeof(Local), "agents", "AgentsCommandDescription", UsageSummaryResourceName = "AgentsCommandUsageSummary", MinArgs = 0, MaxArgs = 1)]
    public class AgentsCommand : Command
    {
        [ImportingConstructor]
        public AgentsCommand(IPackageAgentProvider agentProvider)
        {
            if (agentProvider == null)
                throw new ArgumentNullException("agentProvider");
            AgentProvider = agentProvider;
        }

        private void AddNewAgent(string name, string agent)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new CommandLineException(Local.AgentsCommandNameRequired);
            if (string.Equals(name, Local.ReservedPackageNameAll))
                throw new CommandLineException(Local.AgentsCommandAllNameIsReserved);
            if (string.IsNullOrWhiteSpace(agent))
                throw new CommandLineException(Local.AgentsCommandAgentRequired);
            if (!Utility.IsValidAgent(agent))
                throw new CommandLineException(Local.AgentsCommandInvalidAgent);
            var list = AgentProvider.LoadPackageAgents().ToList();
            if (list.Any(pr => string.Equals(name, pr.Name, StringComparison.OrdinalIgnoreCase)))
                throw new CommandLineException(Local.AgentsCommandUniqueName);
            if (list.Any(pr => string.Equals(agent, pr.Agent, StringComparison.OrdinalIgnoreCase)))
                throw new CommandLineException(Local.AgentsCommandUniqueAgent);
            var item = new PackageAgent(agent, name);
            list.Add(item);
            AgentProvider.SavePackageAgents(list);
            Console.WriteLine(Local.AgentsCommandAgentAddedSuccessfully, new object[] { name });
        }

        private void EnableOrDisableAgent(string name, bool enabled)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new CommandLineException(Local.AgentsCommandNameRequired);
            var agents = AgentProvider.LoadPackageAgents().ToList();
            var agent = agents.Where(ps => string.Equals(name, ps.Name, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!agent.Any())
                throw new CommandLineException(Local.AgentsCommandNoMatchingAgentsFound, new object[] { name });
            agent.ForEach(pa => pa.IsEnabled = enabled);
            AgentProvider.SavePackageAgents(agents);
            Console.WriteLine(enabled ? Local.AgentsCommandAgentEnabledSuccessfully : Local.AgentsCommandAgentDisabledSuccessfully, new object[] { name });
        }

        public override void ExecuteCommand()
        {
            var arg = (Arguments.Any() ? Arguments.First().ToUpperInvariant() : null);
            if (arg != null && arg != "LIST")
            {
                if (arg != "ADD")
                {
                    if (arg != "REMOVE")
                    {
                        if (arg != "ENABLE")
                        {
                            if (arg == "DISABLE")
                                EnableOrDisableAgent(Name, false);
                            return;
                        }
                        EnableOrDisableAgent(Name, true);
                        return;
                    }
                    RemoveAgent(Name);
                    return;
                }
            }
            else
            {
                PrintRegisteredAgents();
                return;
            }
            AddNewAgent(Name, Agent);
        }

        private void PrintRegisteredAgents()
        {
            var list = AgentProvider.LoadPackageAgents().ToList();
            if (!list.Any())
                Console.WriteLine(Local.AgentsCommandNoAgents);
            else
            {
                Console.PrintJustified(0, Local.AgentsCommandRegisteredAgents);
                Console.WriteLine();
                var str = new string(' ', 6);
                for (var i = 0; i < list.Count; i++)
                {
                    var agent = list[i];
                    var num2 = i + 1;
                    var str2 = new string(' ', (i >= 9 ? 1 : 2));
                    Console.WriteLine("  {0}.{1}{2} [{3}]", new object[] { num2, str2, agent.Name, agent.IsEnabled ? Local.AgentsCommandEnabled : Local.AgentsCommandDisabled });
                    Console.WriteLine("{0}{1}", new object[] { str, agent.Agent });
                }
            }
        }

        private void RemoveAgent(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new CommandLineException(Local.AgentsCommandNameRequired);
            var agents = AgentProvider.LoadPackageAgents().ToList();
            var list = agents.Where(pa => string.Equals(name, pa.Name, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!list.Any())
                throw new CommandLineException(Local.AgentsCommandNoMatchingAgentsFound, new object[] { name });
            list.ForEach(pa => agents.Remove(pa));
            AgentProvider.SavePackageAgents(agents);
            Console.WriteLine(Local.AgentsCommandAgentRemovedSuccessfully, new object[] { name });
        }

        [Option(typeof(Local), "AgentsCommandNameDescription")]
        public string Name { get; set; }

        [Option(typeof(Local), "AgentsCommandAgentDescription", AltName = "src")]
        public string Agent { get; set; }

        public IPackageAgentProvider AgentProvider { get; private set; }
    }
}
