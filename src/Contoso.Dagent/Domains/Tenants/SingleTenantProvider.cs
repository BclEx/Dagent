using System;
namespace Contoso.Domains
{
    public class SingleTenantProvider : ITenantProvider
    {
        public string GetApplicationPath(string appId)
        {
            throw new NotImplementedException();
        }

        public string GetServiceAccount(string appId, string descriptor, ServiceAccountMode mode)
        {
            throw new NotImplementedException();
        }
    }
}
