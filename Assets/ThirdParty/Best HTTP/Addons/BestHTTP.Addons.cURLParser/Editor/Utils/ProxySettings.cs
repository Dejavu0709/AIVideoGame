using System;

using BestHTTP.Authentication;

namespace BestHTTP.Addons.cURLParser.Editor.Utils
{
    public sealed class ProxySettings
    {
        public string isTransparent = "false";
        public string noProxyList;
        public string address;
        public string proxyType;

        public AuthenticationTypes auth;
        public string user;
        public string password;
    }
}
