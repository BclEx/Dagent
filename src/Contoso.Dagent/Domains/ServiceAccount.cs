using System;
namespace Contoso.Domains
{
    public static class ServiceAccount
    {
        private static readonly ITenantProvider _tenantProvider = null;

        public static string Create(ApplicationContext appCtx, string descriptor) { return Get(appCtx, descriptor, ServiceAccountMode.Create); }
        public static string Get(ApplicationContext appCtx, string descriptor) { return Get(appCtx, descriptor, ServiceAccountMode.Get); }
        public static string Get(ApplicationContext appCtx, string descriptor, ServiceAccountMode mode)
        {
            return _tenantProvider.GetServiceAccount(appCtx.ID, descriptor, mode);
        }
    }
}
