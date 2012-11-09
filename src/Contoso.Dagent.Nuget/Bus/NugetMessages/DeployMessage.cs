using System;
namespace Contoso.Bus.NugetMessages
{
    public class DeployMessage
    {
        public class Item
        {
            public string PackageId { get; set; }
            public string Version { get; set; }
        }

        public string ApiKey { get; set; }
        public bool WantReply { get; set; }
        public bool FromSpec { get; set; }
        public Item[] Items { get; set; }
        public bool ExcludeVersion { get; set; }
        public bool Prerelease { get; set; }
        public string Email { get; set; }
        public string Project { get; set; }
    }
}