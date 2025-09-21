#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
using System;

using BestHTTP.Addons.TLSSecurity.Databases.ClientCredentials;
using BestHTTP.Addons.TLSSecurity.Databases.OCSP;
using BestHTTP.Addons.TLSSecurity.Databases.X509;

namespace BestHTTP.Addons.TLSSecurity
{
    public sealed class OCSPCacheHTTPRequestOptions
    {
        public int DataLengthThreshold = 256;
        public bool UseKeepAlive = true;
        public bool UseCache = true;

        public TimeSpan ConnectTimeout = TimeSpan.FromSeconds(2);
        public TimeSpan Timeout = TimeSpan.FromSeconds(2);
    }

    public sealed class OCSPCacheOptions
    {
        public TimeSpan MaxWaitTime = TimeSpan.FromSeconds(4);
        public TimeSpan RetryUnknownAfter = TimeSpan.FromMinutes(30);

        public string FolderName = "OCSPCache";

        public OCSPDatabaseOptions DatabaseOptions = new OCSPDatabaseOptions("OCSPStatus");

        public OCSPCacheHTTPRequestOptions HTTPRequestOptions = new OCSPCacheHTTPRequestOptions();
    }

    public sealed class OCSPOptions
    {
        /// <summary>
        /// Enable or disable sending out OCSP requests for revocation checking.
        /// </summary>
        public bool EnableOCSPQueries = true;

        /// <summary>
        /// The addon not going to check revocation status for short lifespan certificates.
        /// </summary>
        public TimeSpan ShortLifeSpanThreshold = TimeSpan.FromDays(10);

        /// <summary>
        /// Treat unknown revocation statuses (unknown OCSP status or unreachable servers) as revoked and abort the TLS negotiation.
        /// </summary>
        public bool FailHard = false;

        /// <summary>
        /// Treat the TLS connection failed if the leaf certificate has the must-staple flag, but the server doesn't send certificate status.
        /// </summary>
        public bool FailOnMissingCertStatusWhenMustStaplePresent = true;

        /// <summary>
        /// OCSP Cache Options
        /// </summary>
        public OCSPCacheOptions OCSPCache = new OCSPCacheOptions();
    }

    public sealed class FolderAndFileOptions
    {
        public string FolderName = "BestHTTP.Addons.TLSSecurity";
        public string DatabaseFolderName = "Databases";
        public string MetadataExtension = "metadata";
        public string DatabaseExtension = "db";
        public string DatabaseFreeListExtension = "dfl";
        public string HashExtension = "hash";
    }

    public static class SecurityOptions
    {
        /// <summary>
        /// If false, only certificates stored in the trusted intermediates database are used to reconstruct the certificate chain. When set to true (default), it improves compatibility but the addon going to use/accept certificates that not stored in its trusted database.
        /// </summary>
        public static bool UseServerSentIntermediateCertificates = true;

        /// <summary>
        /// Folder, file and extension options.
        /// </summary>
        public static FolderAndFileOptions FolderAndFileOptions = new FolderAndFileOptions();

        /// <summary>
        /// OCSP and OCSP cache options.
        /// </summary>
        public static OCSPOptions OCSP = new OCSPOptions();

        /// <summary>
        /// Database options of the Trusted CAs database
        /// </summary>
        public static X509DatabaseOptions TrustedRootsOptions = new X509DatabaseOptions("TrustedRoots");

        /// <summary>
        /// Database options of the Trusted Intermediate Certifications database
        /// </summary>
        public static X509DatabaseOptions TrustedIntermediatesOptions = new X509DatabaseOptions("TrustedIntermediates");

        /// <summary>
        /// Database options of the Client Credentials database
        /// </summary>
        public static ClientCredentialDatabaseOptions ClientCredentialsOptions = new ClientCredentialDatabaseOptions("ClientCredentials");
    }
}

#endif