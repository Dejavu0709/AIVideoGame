#if !BESTHTTP_DISABLE_ALTERNATE_SSL && (!UNITY_WEBGL || UNITY_EDITOR)
using System;
using System.Collections.Generic;
using System.IO;

using BestHTTP.Addons.TLSSecurity.Databases.Shared;
using BestHTTP.PlatformSupport.Memory;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1;
using BestHTTP.SecureProtocol.Org.BouncyCastle.Asn1.X509;
using BestHTTP.SecureProtocol.Org.BouncyCastle.X509;

namespace BestHTTP.Addons.TLSSecurity.Databases.X509
{
    public sealed class x509MetadataService : MetadataService<X509Metadata, X509Certificate>
    {
        public x509MetadataService(X509DatabaseIndexingService indexingService)
            : base(indexingService)
        { }

        public X509Metadata Create(X509Certificate content, MetadataFlags flags, int filePos, int length)
        {
            return CreateDefault(content, flags, filePos, length, (c, metadata) =>
            {
                metadata.Subject = c.SubjectDN;
                metadata.Issuer = c.IssuerDN;
                metadata.SubjectKeyIdentifier = x509MetadataService.GetSubjectKeyIdentifier(c);
            });
        }

        public static byte[] GetSubjectKeyIdentifier(X509Certificate certificate)
        {
            // https://tools.ietf.org/html/rfc5280#section-4.2.1.2
            var ski = SubjectKeyIdentifier.FromExtensions(certificate.CertificateStructure.TbsCertificate.Extensions);

            if (ski != null)
                return ski.GetKeyIdentifier();

            return null;
        }
    }

}
#endif
