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
using Contoso.NuGet;
using NuGet;
using NuGet.Commands;
using NuGet.Common;
namespace Contoso.NuGetCommands
{
    [Command(typeof(Local), "agents", "AgentsCommandDescription", UsageSummaryResourceName = "AgentsCommandUsageSummary", MinArgs = 0, MaxArgs = 1)]
    public class AgentsCommand : Command
    {
        private readonly IPackageAgentProvider _agentProvider;

        [ImportingConstructor]
        public AgentsCommand(IPackageAgentProvider agentProvider)
        {
            if (agentProvider == null)
                throw new ArgumentNullException("agentProvider");
            _agentProvider = agentProvider;
        }

        private void AddNewAgent()
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new CommandLineException(Local.AgentsCommandNameRequired);
            if (string.Equals(Name, Local.ReservedPackageNameAll))
                throw new CommandLineException(Local.AgentsCommandAllNameIsReserved);
            if (string.IsNullOrWhiteSpace(Agent))
                throw new CommandLineException(Local.AgentsCommandAgentRequired);
            if (!Utility.IsValidAgent(Agent))
                throw new CommandLineException(Local.AgentsCommandInvalidAgent);
            var list = _agentProvider.LoadPackageAgents().ToList();
            if (list.Any(pr => string.Equals(Name, pr.Name, StringComparison.OrdinalIgnoreCase)))
                throw new CommandLineException(Local.AgentsCommandUniqueName);
            if (list.Any(pr => string.Equals(Agent, pr.Agent, StringComparison.OrdinalIgnoreCase)))
                throw new CommandLineException(Local.AgentsCommandUniqueAgent);
            var item = new PackageAgent(Agent, Name);
            list.Add(item);
            _agentProvider.SavePackageAgents(list);
            Console.WriteLine(Local.AgentsCommandAgentAddedSuccessfully, new object[] { Name });
        }

        private void EnableOrDisableAgent(bool enabled)
        {
            if (string.IsNullOrEmpty(Name))
                throw new CommandLineException(Local.AgentsCommandNameRequired);
            var agents = _agentProvider.LoadPackageAgents().ToList();
            var agentE = agents.Where(ps => string.Equals(Name, ps.Name, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!agentE.Any())
                throw new CommandLineException(Local.AgentsCommandNoMatchingAgentsFound, new object[] { Name });
            foreach (var agent in agentE)
                agent.IsEnabled = enabled;
            _agentProvider.SavePackageAgents(agents);
            Console.WriteLine(enabled ? Local.AgentsCommandAgentEnabledSuccessfully : Local.AgentsCommandAgentDisabledSuccessfully, new object[] { Name });
        }

        public override void ExecuteCommand()
        {
            var arg = Arguments.FirstOrDefault();
            if (string.IsNullOrEmpty(arg) && arg.Equals("List", StringComparison.OrdinalIgnoreCase))
                PrintRegisteredAgents();
            else if (arg.Equals("Add", StringComparison.OrdinalIgnoreCase))
                AddNewAgent();
            else if (arg.Equals("Remove", StringComparison.OrdinalIgnoreCase))
                RemoveAgent();
            else if (arg.Equals("Enable", StringComparison.OrdinalIgnoreCase))
                EnableOrDisableAgent(true);
            else if (arg.Equals("Disable", StringComparison.OrdinalIgnoreCase))
                EnableOrDisableAgent(false);
            else if (arg.Equals("Update", StringComparison.OrdinalIgnoreCase))
                UpdatePackageAgent();
        }

        private void PrintRegisteredAgents()
        {
            var list = _agentProvider.LoadPackageAgents().ToList();
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

        private void RemoveAgent()
        {
            if (string.IsNullOrEmpty(Name))
                throw new CommandLineException(Local.AgentsCommandNameRequired);
            var agents = _agentProvider.LoadPackageAgents().ToList();
            var agentE = agents.Where(pa => string.Equals(Name, pa.Name, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!agentE.Any())
                throw new CommandLineException(Local.AgentsCommandNoMatchingAgentsFound, new object[] { Name });
            agents.RemoveAll(new Predicate<PackageAgent>(agentE.Contains));
            _agentProvider.SavePackageAgents(agents);
            Console.WriteLine(Local.AgentsCommandAgentRemovedSuccessfully, new object[] { Name });
        }

        private void UpdatePackageAgent()
        {
            Func<PackageAgent, bool> predicate = null;
            if (string.IsNullOrEmpty(Name))
                throw new CommandLineException(Local.AgentsCommandNameRequired);
            var agents = _agentProvider.LoadPackageAgents().ToList();
            int index = agents.FindIndex(pa => Name.Equals(pa.Name, StringComparison.OrdinalIgnoreCase));
            if (index == -1)
                throw new CommandLineException(Local.AgentsCommandNoMatchingAgentsFound, new object[] { Name });
            var item = agents[index];
            if (!string.IsNullOrEmpty(Agent) && !item.Agent.Equals(Agent, StringComparison.OrdinalIgnoreCase))
            {
                if (!Utility.IsValidAgent(Agent))
                    throw new CommandLineException(Local.AgentsCommandInvalidAgent);
                if (predicate == null)
                    predicate = pa => string.Equals(Agent, pa.Agent, StringComparison.OrdinalIgnoreCase);
                if (agents.Any(predicate))
                    throw new CommandLineException(Local.AgentsCommandUniqueAgent);
                item = new PackageAgent(Agent, item.Name);
            }
            agents.RemoveAt(index);
            agents.Insert(index, item);
            _agentProvider.SavePackageAgents(agents);
            Console.WriteLine(Local.AgentsCommandUpdateSuccessful, new object[] { Name });
        }

        [Option(typeof(Local), "AgentsCommandNameDescription")]
        public string Name { get; set; }

        [Option(typeof(Local), "AgentsCommandAgentDescription", AltName = "src")]
        public string Agent { get; set; }
    }
}
