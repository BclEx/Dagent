using System;
namespace Contoso.Domains
{
    public interface ITenantProvider
    {
        string GetApplicationPath(string appId);
        string GetServiceAccount(string appId, string descriptor, ServiceAccountMode mode);
    }
}
