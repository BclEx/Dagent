using System;
namespace Contoso.Domains
{
    public class ApplicationContext
    {
        private static readonly ITenantProvider _tenantProvider = null;

        public ApplicationContext(string id)
        {
            ID = id;
            Path = _tenantProvider.GetApplicationPath(id);
        }

        public string ID { get; private set; }
        public string Path { get; private set; }
    }
}
