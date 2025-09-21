#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)

using BestHTTP.Addons.TLSSecurity.Databases.Shared;

namespace BestHTTP.Addons.TLSSecurity.Databases.ClientCredentials
{
    public sealed class ClientCredentialDatabaseOptions : DatabaseOptions
    {
        public ClientCredentialDatabaseOptions(string dbName)
            : base(dbName)
        {
            this.UseHashFile = true;
        }
    }
}
#endif
