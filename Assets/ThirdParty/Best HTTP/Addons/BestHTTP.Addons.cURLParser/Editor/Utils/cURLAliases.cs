using System;

// Some of the types are mirrored from https://github.com/curl/curl/blob/master/src/tool_getparam.c

namespace BestHTTP.Addons.cURLParser.Editor.Utils
{
    public enum ARGTypes
    {
        ARG_NONE,   /* stand-alone but not a boolean */
        ARG_BOOL,   /* accepts a --no-[name] prefix */
        ARG_STRING, /* requires an argument */
        ARG_FILENAME /* requires an argument, usually a file name */
    };

    public struct LongShort
    {
        public static readonly LongShort Empty = new LongShort(null, null, ARGTypes.ARG_NONE);

        public string letter; /* short name option */
        public string lname;  /* long name option */
        public ARGTypes desc;

        public LongShort(string l, string ln, ARGTypes aRGType)
        {
            this.letter = l;
            this.lname = ln;
            this.desc = aRGType;
        }
    };

    public static class cURLAliases{

        public static LongShort FindByShort(char shortAlias)
        {
            int count = aliases.Length;

            for (int i = 0; i < count; i++)
            {
                var alias = aliases[i];

                if (alias.letter.Length > 1)
                    continue;

                if (alias.letter[0] == shortAlias)
                    return alias;
            }

            return LongShort.Empty;
        }

        public static LongShort FindByLong(string longAlias)
        {
            int count = aliases.Length;
            
            for (int i = 0; i < count; i++)
            {
                var alias = aliases[i];

                if (alias.lname.Equals(longAlias, StringComparison.OrdinalIgnoreCase))
                    return alias;
            }

            return LongShort.Empty;
        }

        static LongShort[] aliases = new LongShort[] {
              /* 'letter' strings with more than one character have *no* short option to
                 mention. */
              new LongShort( "*@", "url",                      ARGTypes.ARG_STRING),
              new LongShort( "*4", "dns-ipv4-addr",            ARGTypes.ARG_STRING),
              new LongShort( "*6", "dns-ipv6-addr",            ARGTypes.ARG_STRING),
              new LongShort( "*a", "random-file",              ARGTypes.ARG_FILENAME),
              new LongShort( "*b", "egd-file",                 ARGTypes.ARG_STRING),
              new LongShort( "*B", "oauth2-bearer",            ARGTypes.ARG_STRING),
              new LongShort( "*c", "connect-timeout",          ARGTypes.ARG_STRING),
              new LongShort( "*C", "doh-url"        ,          ARGTypes.ARG_STRING),
              new LongShort( "*d", "ciphers",                  ARGTypes.ARG_STRING),
              new LongShort( "*D", "dns-interface",            ARGTypes.ARG_STRING),
              new LongShort( "*e", "disable-epsv",             ARGTypes.ARG_BOOL),
              new LongShort( "*f", "disallow-username-in-url", ARGTypes.ARG_BOOL),
              new LongShort( "*E", "epsv",                     ARGTypes.ARG_BOOL),

              new LongShort( "*F", "dns-servers",              ARGTypes.ARG_STRING),
              new LongShort( "*g", "trace",                    ARGTypes.ARG_FILENAME),
              new LongShort( "*G", "npn",                      ARGTypes.ARG_BOOL),
              new LongShort( "*h", "trace-ascii",              ARGTypes.ARG_FILENAME),
              new LongShort( "*H", "alpn",                     ARGTypes.ARG_BOOL),
              new LongShort( "*i", "limit-rate",               ARGTypes.ARG_STRING),
              new LongShort( "*j", "compressed",               ARGTypes.ARG_BOOL),
              new LongShort( "*J", "tr-encoding",              ARGTypes.ARG_BOOL),
              new LongShort( "*k", "digest",                   ARGTypes.ARG_BOOL),
              new LongShort( "*l", "negotiate",                ARGTypes.ARG_BOOL),
              new LongShort( "*m", "ntlm",                     ARGTypes.ARG_BOOL),
              new LongShort( "*M", "ntlm-wb",                  ARGTypes.ARG_BOOL),
              new LongShort( "*n", "basic",                    ARGTypes.ARG_BOOL),
              new LongShort( "*o", "anyauth",                  ARGTypes.ARG_BOOL),

              new LongShort( "*p", "wdebug",                   ARGTypes.ARG_BOOL),
              new LongShort( "*q", "ftp-create-dirs",          ARGTypes.ARG_BOOL),
              new LongShort( "*r", "create-dirs",              ARGTypes.ARG_BOOL),
              new LongShort( "*R", "create-file-mode",         ARGTypes.ARG_STRING),
              new LongShort( "*s", "max-redirs",               ARGTypes.ARG_STRING),
              new LongShort( "*t", "proxy-ntlm",               ARGTypes.ARG_BOOL),
              new LongShort( "*u", "crlf",                     ARGTypes.ARG_BOOL),
              new LongShort( "*v", "stderr",                   ARGTypes.ARG_FILENAME),
              new LongShort( "*V", "aws-sigv4",                ARGTypes.ARG_STRING),
              new LongShort( "*w", "interface",                ARGTypes.ARG_STRING),
              new LongShort( "*x", "krb",                      ARGTypes.ARG_STRING),
              new LongShort( "*x", "krb4",                     ARGTypes.ARG_STRING),

              new LongShort( "*X", "haproxy-protocol",         ARGTypes.ARG_BOOL),
              new LongShort( "*y", "max-filesize",             ARGTypes.ARG_STRING),
              new LongShort( "*z", "disable-eprt",             ARGTypes.ARG_BOOL),
              new LongShort( "*Z", "eprt",                     ARGTypes.ARG_BOOL),

              new LongShort( "*~", "xattr",                    ARGTypes.ARG_BOOL),
              new LongShort( "$a", "ftp-ssl",                  ARGTypes.ARG_BOOL),
                     
              new LongShort( "$a", "ssl",                      ARGTypes.ARG_BOOL),
                     
              new LongShort( "$b", "ftp-pasv",                 ARGTypes.ARG_BOOL),
              new LongShort( "$c", "socks5",                   ARGTypes.ARG_STRING),
              new LongShort( "$d", "tcp-nodelay",              ARGTypes.ARG_BOOL),
              new LongShort( "$e", "proxy-digest",             ARGTypes.ARG_BOOL),
              new LongShort( "$f", "proxy-basic",              ARGTypes.ARG_BOOL),
              new LongShort( "$g", "retry",                    ARGTypes.ARG_STRING),
              new LongShort( "$V", "retry-connrefused",        ARGTypes.ARG_BOOL),
              new LongShort( "$h", "retry-delay",              ARGTypes.ARG_STRING),
              new LongShort( "$i", "retry-max-time",           ARGTypes.ARG_STRING),
              new LongShort( "$k", "proxy-negotiate",          ARGTypes.ARG_BOOL),
              new LongShort( "$m", "ftp-account",              ARGTypes.ARG_STRING),
              new LongShort( "$n", "proxy-anyauth",            ARGTypes.ARG_BOOL),
              new LongShort( "$o", "trace-time",               ARGTypes.ARG_BOOL),
              new LongShort( "$p", "ignore-content-length",    ARGTypes.ARG_BOOL),
              new LongShort( "$q", "ftp-skip-pasv-ip",         ARGTypes.ARG_BOOL),
              new LongShort( "$r", "ftp-method",               ARGTypes.ARG_STRING),
              new LongShort( "$s", "local-port",               ARGTypes.ARG_STRING),
              new LongShort( "$t", "socks4",                   ARGTypes.ARG_STRING),
              new LongShort( "$T", "socks4a",                  ARGTypes.ARG_STRING),
              new LongShort( "$u", "ftp-alternative-to-user",  ARGTypes.ARG_STRING),
              new LongShort( "$v", "ftp-ssl-reqd",             ARGTypes.ARG_BOOL),
                     
              new LongShort( "$v", "ssl-reqd",                 ARGTypes.ARG_BOOL),
                     
              new LongShort( "$w", "sessionid",                ARGTypes.ARG_BOOL),
                     
              new LongShort( "$x", "ftp-ssl-control",          ARGTypes.ARG_BOOL),
              new LongShort( "$y", "ftp-ssl-ccc",              ARGTypes.ARG_BOOL),
              new LongShort( "$j", "ftp-ssl-ccc-mode",         ARGTypes.ARG_STRING),
              new LongShort( "$z", "libcurl",                  ARGTypes.ARG_STRING),
              new LongShort( "$#", "raw",                      ARGTypes.ARG_BOOL),
              new LongShort( "$0", "post301",                  ARGTypes.ARG_BOOL),
              new LongShort( "$1", "keepalive",                ARGTypes.ARG_BOOL),
                     
              new LongShort( "$2", "socks5-hostname",          ARGTypes.ARG_STRING),
              new LongShort( "$3", "keepalive-time",           ARGTypes.ARG_STRING),
              new LongShort( "$4", "post302",                  ARGTypes.ARG_BOOL),
              new LongShort( "$5", "noproxy",                  ARGTypes.ARG_STRING),
              new LongShort( "$7", "socks5-gssapi-nec",        ARGTypes.ARG_BOOL),
              new LongShort( "$8", "proxy1.0",                 ARGTypes.ARG_STRING),
              new LongShort( "$9", "tftp-blksize",             ARGTypes.ARG_STRING),
              new LongShort( "$A", "mail-from",                ARGTypes.ARG_STRING),
              new LongShort( "$B", "mail-rcpt",                ARGTypes.ARG_STRING),
              new LongShort( "$C", "ftp-pret",                 ARGTypes.ARG_BOOL),
              new LongShort( "$D", "proto",                    ARGTypes.ARG_STRING),
              new LongShort( "$E", "proto-redir",              ARGTypes.ARG_STRING),
              new LongShort( "$F", "resolve",                  ARGTypes.ARG_STRING),
              new LongShort( "$G", "delegation",               ARGTypes.ARG_STRING),
              new LongShort( "$H", "mail-auth",                ARGTypes.ARG_STRING),
              new LongShort( "$I", "post303",                  ARGTypes.ARG_BOOL),
              new LongShort( "$J", "metalink",                 ARGTypes.ARG_BOOL),
              new LongShort( "$6", "sasl-authzid",             ARGTypes.ARG_STRING),
              new LongShort( "$K", "sasl-ir",                  ARGTypes.ARG_BOOL ),
              new LongShort( "$L", "test-event",               ARGTypes.ARG_BOOL),
              new LongShort( "$M", "unix-socket",              ARGTypes.ARG_FILENAME),
              new LongShort( "$N", "path-as-is",               ARGTypes.ARG_BOOL),
              new LongShort( "$O", "socks5-gssapi-service",    ARGTypes.ARG_STRING),
                     
              new LongShort( "$O", "proxy-service-name",       ARGTypes.ARG_STRING),
              new LongShort( "$P", "service-name",             ARGTypes.ARG_STRING),
              new LongShort( "$Q", "proto-default",            ARGTypes.ARG_STRING),
              new LongShort( "$R", "expect100-timeout",        ARGTypes.ARG_STRING),
              new LongShort( "$S", "tftp-no-options",          ARGTypes.ARG_BOOL),
              new LongShort( "$U", "connect-to",               ARGTypes.ARG_STRING),
              new LongShort( "$W", "abstract-unix-socket",     ARGTypes.ARG_FILENAME),
              new LongShort( "$X", "tls-max",                  ARGTypes.ARG_STRING),
              new LongShort( "$Y", "suppress-connect-headers", ARGTypes.ARG_BOOL),
              new LongShort( "$Z", "compressed-ssh",           ARGTypes.ARG_BOOL),
              new LongShort( "$~", "happy-eyeballs-timeout-ms",ARGTypes.ARG_STRING),
              new LongShort( "$!", "retry-all-errors",         ARGTypes.ARG_BOOL),
              new LongShort( "0",   "http1.0",                 ARGTypes.ARG_NONE),
              new LongShort( "01",  "http1.1",                 ARGTypes.ARG_NONE),
              new LongShort( "02",  "http2",                   ARGTypes.ARG_NONE),
              new LongShort( "03",  "http2-prior-knowledge",   ARGTypes.ARG_NONE),
              new LongShort( "04",  "http3",                   ARGTypes.ARG_NONE),
              new LongShort( "09",  "http0.9",                 ARGTypes.ARG_BOOL),
              new LongShort( "1",  "tlsv1",                    ARGTypes.ARG_NONE),
              new LongShort( "10",  "tlsv1.0",                 ARGTypes.ARG_NONE),
              new LongShort( "11",  "tlsv1.1",                 ARGTypes.ARG_NONE),
              new LongShort( "12",  "tlsv1.2",                 ARGTypes.ARG_NONE),
              new LongShort( "13",  "tlsv1.3",                 ARGTypes.ARG_NONE),
              new LongShort( "1A", "tls13-ciphers",            ARGTypes.ARG_STRING),
              new LongShort( "1B", "proxy-tls13-ciphers",      ARGTypes.ARG_STRING),
              new LongShort( "2",  "sslv2",                    ARGTypes.ARG_NONE),
              new LongShort( "3",  "sslv3",                    ARGTypes.ARG_NONE),
              new LongShort( "4",  "ipv4",                     ARGTypes.ARG_NONE),
              new LongShort( "6",  "ipv6",                     ARGTypes.ARG_NONE),
              new LongShort( "a",  "append",                   ARGTypes.ARG_BOOL),
              new LongShort( "A",  "user-agent",               ARGTypes.ARG_STRING),
              new LongShort( "b",  "cookie",                   ARGTypes.ARG_STRING),
              new LongShort( "ba", "alt-svc",                  ARGTypes.ARG_STRING),
              new LongShort( "bb", "hsts",                     ARGTypes.ARG_STRING),
              new LongShort( "B",  "use-ascii",                ARGTypes.ARG_BOOL),
              new LongShort( "c",  "cookie-jar",               ARGTypes.ARG_STRING),
              new LongShort( "C",  "continue-at",              ARGTypes.ARG_STRING),
              new LongShort( "d",  "data",                     ARGTypes.ARG_STRING),
              new LongShort( "dr", "data-raw",                 ARGTypes.ARG_STRING),
              new LongShort( "da", "data-ascii",               ARGTypes.ARG_STRING),
              new LongShort( "db", "data-binary",              ARGTypes.ARG_STRING),
              new LongShort( "de", "data-urlencode",           ARGTypes.ARG_STRING),
              new LongShort( "D",  "dump-header",              ARGTypes.ARG_FILENAME),
              new LongShort( "e",  "referer",                  ARGTypes.ARG_STRING),
              new LongShort( "E",  "cert",                     ARGTypes.ARG_FILENAME),
              new LongShort( "Ea", "cacert",                   ARGTypes.ARG_FILENAME),
              new LongShort( "Eb", "cert-type",                ARGTypes.ARG_STRING),
              new LongShort( "Ec", "key",                      ARGTypes.ARG_FILENAME),
              new LongShort( "Ed", "key-type",                 ARGTypes.ARG_STRING),
              new LongShort( "Ee", "pass",                     ARGTypes.ARG_STRING),
              new LongShort( "Ef", "engine",                   ARGTypes.ARG_STRING),
              new LongShort( "Eg", "capath",                   ARGTypes.ARG_FILENAME),
              new LongShort( "Eh", "pubkey",                   ARGTypes.ARG_STRING),
              new LongShort( "Ei", "hostpubmd5",               ARGTypes.ARG_STRING),
              new LongShort( "Ej", "crlfile",                  ARGTypes.ARG_FILENAME),
              new LongShort( "Ek", "tlsuser",                  ARGTypes.ARG_STRING),
              new LongShort( "El", "tlspassword",              ARGTypes.ARG_STRING),
              new LongShort( "Em", "tlsauthtype",              ARGTypes.ARG_STRING),
              new LongShort( "En", "ssl-allow-beast",          ARGTypes.ARG_BOOL),
              
              new LongShort( "Ep", "pinnedpubkey",             ARGTypes.ARG_STRING),
              new LongShort( "EP", "proxy-pinnedpubkey",       ARGTypes.ARG_STRING),
              new LongShort( "Eq", "cert-status",              ARGTypes.ARG_BOOL),
              new LongShort( "Er", "false-start",              ARGTypes.ARG_BOOL),
              new LongShort( "Es", "ssl-no-revoke",            ARGTypes.ARG_BOOL),
              new LongShort( "ES", "ssl-revoke-best-effort",   ARGTypes.ARG_BOOL),
              new LongShort( "Et", "tcp-fastopen",             ARGTypes.ARG_BOOL),
              new LongShort( "Eu", "proxy-tlsuser",            ARGTypes.ARG_STRING),
              new LongShort( "Ev", "proxy-tlspassword",        ARGTypes.ARG_STRING),
              new LongShort( "Ew", "proxy-tlsauthtype",        ARGTypes.ARG_STRING),
              new LongShort( "Ex", "proxy-cert",               ARGTypes.ARG_FILENAME),
              new LongShort( "Ey", "proxy-cert-type",          ARGTypes.ARG_STRING),
              new LongShort( "Ez", "proxy-key",                ARGTypes.ARG_FILENAME),
              new LongShort( "E0", "proxy-key-type",           ARGTypes.ARG_STRING),
              new LongShort( "E1", "proxy-pass",               ARGTypes.ARG_STRING),
              new LongShort( "E2", "proxy-ciphers",            ARGTypes.ARG_STRING),
              new LongShort( "E3", "proxy-crlfile",            ARGTypes.ARG_FILENAME),
              new LongShort( "E4", "proxy-ssl-allow-beast",    ARGTypes.ARG_BOOL),
              new LongShort( "E5", "login-options",            ARGTypes.ARG_STRING),
              new LongShort( "E6", "proxy-cacert",             ARGTypes.ARG_FILENAME),
              new LongShort( "E7", "proxy-capath",             ARGTypes.ARG_FILENAME),
              new LongShort( "E8", "proxy-insecure",           ARGTypes.ARG_BOOL),
              new LongShort( "E9", "proxy-tlsv1",              ARGTypes.ARG_NONE),
              new LongShort( "EA", "socks5-basic",             ARGTypes.ARG_BOOL),
              new LongShort( "EB", "socks5-gssapi",            ARGTypes.ARG_BOOL),
              new LongShort( "EC", "etag-save",                ARGTypes.ARG_FILENAME),
              new LongShort( "ED", "etag-compare",             ARGTypes.ARG_FILENAME),
              new LongShort( "EE", "curves",                   ARGTypes.ARG_STRING),
              new LongShort( "f",  "fail",                     ARGTypes.ARG_BOOL),
              new LongShort( "fa", "fail-early",               ARGTypes.ARG_BOOL),
              new LongShort( "fb", "styled-output",            ARGTypes.ARG_BOOL),
              new LongShort( "fc", "mail-rcpt-allowfails",     ARGTypes.ARG_BOOL),
              new LongShort( "F",  "form",                     ARGTypes.ARG_STRING),
              new LongShort( "Fs", "form-string",              ARGTypes.ARG_STRING),
              new LongShort( "g",  "globoff",                  ARGTypes.ARG_BOOL),
              new LongShort( "G",  "get",                      ARGTypes.ARG_NONE),
              new LongShort( "Ga", "request-target",           ARGTypes.ARG_STRING),
              new LongShort( "h",  "help",                     ARGTypes.ARG_BOOL),
              new LongShort( "H",  "header",                   ARGTypes.ARG_STRING),
              new LongShort( "Hp", "proxy-header",             ARGTypes.ARG_STRING),
              new LongShort( "i",  "include",                  ARGTypes.ARG_BOOL),
              new LongShort( "I",  "head",                     ARGTypes.ARG_BOOL),
              new LongShort( "j",  "junk-session-cookies",     ARGTypes.ARG_BOOL),
              new LongShort( "J",  "remote-header-name",       ARGTypes.ARG_BOOL),
              new LongShort( "k",  "insecure",                 ARGTypes.ARG_BOOL),
              new LongShort( "K",  "config",                   ARGTypes.ARG_FILENAME),
              new LongShort( "l",  "list-only",                ARGTypes.ARG_BOOL),
              new LongShort( "L",  "location",                 ARGTypes.ARG_BOOL),
              new LongShort( "Lt", "location-trusted",         ARGTypes.ARG_BOOL),
              new LongShort( "m",  "max-time",                 ARGTypes.ARG_STRING),
              new LongShort( "M",  "manual",                   ARGTypes.ARG_BOOL),
              new LongShort( "n",  "netrc",                    ARGTypes.ARG_BOOL),
              new LongShort( "no", "netrc-optional",           ARGTypes.ARG_BOOL),
              new LongShort( "ne", "netrc-file",               ARGTypes.ARG_FILENAME),
              new LongShort( "N",  "buffer",                   ARGTypes.ARG_BOOL),
                     
              new LongShort( "o",  "output",                   ARGTypes.ARG_FILENAME),
              new LongShort( "O",  "remote-name",              ARGTypes.ARG_NONE),
              new LongShort( "Oa", "remote-name-all",          ARGTypes.ARG_BOOL),
              new LongShort( "Ob", "output-dir",               ARGTypes.ARG_STRING),
              new LongShort( "p",  "proxytunnel",              ARGTypes.ARG_BOOL),
              new LongShort( "P",  "ftp-port",                 ARGTypes.ARG_STRING),
              new LongShort( "q",  "disable",                  ARGTypes.ARG_BOOL),
              new LongShort( "Q",  "quote",                    ARGTypes.ARG_STRING),
              new LongShort( "r",  "range",                    ARGTypes.ARG_STRING),
              new LongShort( "R",  "remote-time",              ARGTypes.ARG_BOOL),
              new LongShort( "s",  "silent",                   ARGTypes.ARG_BOOL),
              new LongShort( "S",  "show-error",               ARGTypes.ARG_BOOL),
              new LongShort( "t",  "telnet-option",            ARGTypes.ARG_STRING),
              new LongShort( "T",  "upload-file",              ARGTypes.ARG_FILENAME),
              new LongShort( "u",  "user",                     ARGTypes.ARG_STRING),
              new LongShort( "U",  "proxy-user",               ARGTypes.ARG_STRING),
              new LongShort( "v",  "verbose",                  ARGTypes.ARG_BOOL),
              new LongShort( "V",  "version",                  ARGTypes.ARG_BOOL),
              new LongShort( "w",  "write-out",                ARGTypes.ARG_STRING),
              new LongShort( "x",  "proxy",                    ARGTypes.ARG_STRING),
              new LongShort( "xa", "preproxy",                 ARGTypes.ARG_STRING),
              new LongShort( "X",  "request",                  ARGTypes.ARG_STRING),
              new LongShort( "Y",  "speed-limit",              ARGTypes.ARG_STRING),
              new LongShort( "y",  "speed-time",               ARGTypes.ARG_STRING),
              new LongShort( "z",  "time-cond",                ARGTypes.ARG_STRING),
              new LongShort( "Z",  "parallel",                 ARGTypes.ARG_BOOL),
              new LongShort( "Zb", "parallel-max",             ARGTypes.ARG_STRING),
              new LongShort( "Zc", "parallel-immediate",       ARGTypes.ARG_BOOL),
              new LongShort( "#",  "progress-bar",             ARGTypes.ARG_BOOL),
              new LongShort( "#m", "progress-meter",           ARGTypes.ARG_BOOL),
              new LongShort( ":",  "next",                     ARGTypes.ARG_NONE)
        };
    }
}
