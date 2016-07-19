using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Xml;
using Microsoft.IdentityModel.Protocols.Saml2;
using Microsoft.IdentityModel.Protocols.Saml2.Constants;
using Microsoft.IdentityModel.SecurityTokenService;
using Microsoft.IdentityModel.Tokens.Saml2;

namespace ExchangePortal
{
    /// <summary>
    /// Utility class for reading a SAML2.0 SSO token
    /// </summary>
    public static class TokenHandler
    {
        /// <summary>
        /// SAML Key for retrieving the signing certificate thumbprint
        /// </summary>
        public const string SamlKeyThumbprint = "thumbprint";

        /// <summary>
        /// Default URI
        /// </summary>
        public const string DefaultAttrUri = "urn:oasis:names:tc:SAML:2.0:attrname-format:unspecified";

        /// <summary>
        /// The signature algorithm
        /// </summary>
        public const string SignatureAlgorithm = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";

        /// <summary>
        /// The digest algorithm
        /// </summary>
        public const string DigestAlgorithm = "http://www.w3.org/2000/09/xmldsig#sha1";

        /// <summary>
        /// The signing certificate
        /// </summary>
        public static X509Certificate2 SigningCertificate = null;

        /// <summary>
        /// Reads a SAML 2.0 SSO token 
        /// </summary>
        /// <param name="token">SAML xml token</param>
        /// <returns>Token data</returns>
        public static Dictionary<string, string> Read(string token)
        {
            // load a new xml document
            var xmldoc = new XmlDocument { PreserveWhitespace = true };
            xmldoc.LoadXml(token);

            var ns = new XmlNamespaceManager(xmldoc.NameTable);
            ns.AddNamespace("samlp", @"urn:oasis:names:tc:SAML:2.0:protocol");
            ns.AddNamespace("saml", @"urn:oasis:names:tc:SAML:2.0:assertion");
            ns.AddNamespace("dsig", @"http://www.w3.org/2000/09/xmldsig#");

            // read required nodes from saml response xml
            var responseNode = xmldoc.SelectSingleRequiredNode("/samlp:Response", ns);
            var signatureNode = responseNode.SelectSingleRequiredNode("dsig:Signature", ns);
            var certificateNode = signatureNode.SelectSingleRequiredNode("dsig:KeyInfo/dsig:X509Data/dsig:X509Certificate", ns);

            // load the xml signature
            var signedXml = new SignedXml(xmldoc.DocumentElement);
            signedXml.LoadXml((XmlElement) signatureNode);

            // create the certificate and verify that it's trusted
            var certificate = new X509Certificate2(Convert.FromBase64String(certificateNode.InnerText));
            var store = new X509Store(StoreName.TrustedPeople, StoreLocation.LocalMachine);
            store.Open(OpenFlags.ReadOnly);

            // look for certificate in trusted store
            var certs = store.Certificates.Find(
                X509FindType.FindByThumbprint,
                certificate.Thumbprint,
                true);
            if (certs.Count == 0)
            {
                throw new InvalidOperationException(
                    string.Format(
                        "Certificate not found in trusted store Thumbprint={0}",
                        certificate.Thumbprint));
            }

            // check the key and signature match
            var isSigned = signedXml.CheckSignature(certificate, true);
            if (!isSigned)
            {
                throw new XmlException("Signature check failed");
            }

            // read required data
            var assertionNode = responseNode.SelectSingleRequiredNode("saml:Assertion", ns);
            var paymentTransactionId = assertionNode.SelectSingleRequiredNode("//saml:Attribute[@Name='Payment Transaction ID']/saml:AttributeValue", ns);
            var totalAmountPaid = assertionNode.SelectSingleRequiredNode("//saml:Attribute[@Name='Total Amount Paid']/saml:AttributeValue", ns);
            var paymentStatus = assertionNode.SelectSingleRequiredNode("//saml:Attribute[@Name='Payment Status']/saml:AttributeValue", ns);


            return new Dictionary<string, string>()
            {
               {"paymentTransactionID", paymentTransactionId.InnerText },
               {"totalAmountPaid", totalAmountPaid.InnerText },
               {"paymentStatus", paymentStatus.InnerText },
            };
        }

        /// <summary>
        /// Writes a SAML 2 Response
        /// </summary>
        /// <param name="assertions">SAML 2.0 Assertion to be inserted into the response</param>
        /// <param name="issuerName">Name of the issuer.</param>
        /// <param name="subjectName">Name of the subject.</param>
        /// <param name="certId">X509 Certificate Thumbprint to retrieve the certificate by</param>
        /// <param name="storeName">Name of the certificate store.</param>
        /// <param name="storeLocation">The certificate store location.</param>
        /// <returns></returns>
        public static string Write(Dictionary<string, string> assertions, string issuerName, string subjectName, string certId, StoreName storeName, StoreLocation storeLocation )
        {
            //Create the SAML assertion
            var assertion = CreateSamlAssertion(assertions, issuerName, subjectName, certId, storeName, storeLocation);

            //The SAML response
            var response = CreateSamlResponse(assertion, issuerName, certId, storeName, storeLocation);

            var serializedResponse = SerializeSamlResponse(response);

            return serializedResponse;
        }

        /// <summary>
        /// Extension method on XmlNode that executes an xpath query and throws an XmlException if node is not found
        /// </summary>
        /// <param name="node">Xml node instance</param>
        /// <param name="xpath">XPath query</param>
        /// <param name="ns">namesapce manager</param>
        /// <returns>XmlNode instance</returns>
        public static XmlNode SelectSingleRequiredNode(this XmlNode node, string xpath, XmlNamespaceManager ns)
        {
            var n = node.SelectSingleNode(xpath, ns);
            if (n == null)
            {
                throw new XmlException(xpath + "returned zero elements");
            }
            return n;
        }

        /// <summary>
        /// Creates a SAML Response and includes the assertion provided and signs with the certificate thumbprint.
        /// </summary>
        /// <param name="assertion">SAML 2.0 Assertion to be inserted into the response</param>
        /// <param name="issuerName">Name of the issuer.</param>
        /// <param name="certId">X509 Certificate Thumbprint to retrieve the certificate by</param>
        /// <param name="storeName">Name of the certificate store.</param>
        /// <param name="storeLocation">The certificate store location.</param>
        /// <returns>
        /// Signed SAML Response with appropriate SAML assertion
        /// </returns>
        public static Response CreateSamlResponse(Saml2Assertion assertion, string issuerName, string certId, StoreName storeName, StoreLocation storeLocation)
        {
            var response = new Response(new Status(StatusCodes.Success))
            {
                Issuer = new Saml2NameIdentifier(issuerName),
                Status = { StatusMessage = "Success" },
            };

            response.Assertions.Add(assertion);

            var certificate = FindCertificate(certId, storeName, storeLocation);
            response.SigningCredentials = new X509SigningCredentials(certificate, SignatureAlgorithm, DigestAlgorithm);

            return response;
        }

        /// <summary>
        /// Creates a SAML 2.0 assertion applying the attributes provided and signed with the certificate thumbprint
        /// </summary>
        /// <param name="attributes">Dictionary of attributes to be added to the SAML assertion</param>
        /// <param name="issuerName">Name of the issuer.</param>
        /// <param name="subjectName">Name of the subject.</param>
        /// <param name="certId">The certificate thumbprint used to retrieve the X509 certificate by</param>
        /// <param name="storeName">Name of the certificate store.</param>
        /// <param name="storeLocation">The certificate store location.</param>
        /// <returns>
        /// Signed SAML 2.0 assertion with the appropriate attributes
        /// </returns>
        public static Saml2Assertion CreateSamlAssertion(Dictionary<string, string> attributes, string issuerName, string subjectName, string certId, StoreName storeName, StoreLocation storeLocation)
        {
            var identifier = new Saml2NameIdentifier(issuerName) { Format = Saml2Constants.NameIdentifierFormats.Unspecified };
            var assertion = new Saml2Assertion(identifier)
            {
                Id = new Saml2Id("SamlAssertion-" + Guid.NewGuid().ToString().Replace("-", "")),
                Subject = new Saml2Subject(new Saml2NameIdentifier(subjectName)
                {
                    Format = Saml2Constants.NameIdentifierFormats.Unspecified
                }),
                IssueInstant = DateTime.UtcNow,
            };

            var subjectConf = new Saml2SubjectConfirmation(Saml2Constants.ConfirmationMethods.SenderVouches);

            assertion.Subject.SubjectConfirmations.Add(subjectConf);

            assertion.Conditions = new Saml2Conditions
            {
                NotBefore = DateTime.Now.AddMinutes(-2),
                NotOnOrAfter = DateTime.Now.AddMinutes(5)
            };

            var attrStatement = new Saml2AttributeStatement();

            foreach (var key in attributes.Keys)
            {
                if (string.IsNullOrEmpty(attributes[key]))
                {
                    continue;
                }

                var attr = CreateSamlAttribute(key, attributes[key], DefaultAttrUri);
                attrStatement.Attributes.Add(attr);
            }

            assertion.Statements.Add(attrStatement);

            var certificate = FindCertificate(certId, storeName, storeLocation);
            assertion.SigningCredentials = new X509SigningCredentials(certificate, SignatureAlgorithm, DigestAlgorithm);

            return assertion;
        }

        /// <summary>
        /// Serialize the SAML 2.0 response as an XML string
        /// </summary>
        /// <param name="response">The SAML 2.0 response</param>
        /// <returns>String containing the XML representing the SAML 2.0 response</returns>
        public static string SerializeSamlResponse(Response response)
        {
            using (var stringWriter = new StringWriter())
            {
                var settings = new XmlWriterSettings
                {
                    OmitXmlDeclaration = true,
                    ConformanceLevel = ConformanceLevel.Fragment,
                    Encoding = Encoding.UTF8
                };

                using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
                {
                    var serializer = new Saml2ProtocolSerializer();
                    serializer.WriteMessage(xmlWriter, response);

                    return stringWriter.GetStringBuilder().ToString();
                }
            }
        }

        /// <summary>
        /// Locates the X509 certificate specified by the thumbprint provided
        /// </summary>
        /// <param name="certName">The certificate thumbprint</param>
        /// <param name="storeName">Name of the certificate store.</param>
        /// <param name="storeLocation">The certificate store location.</param>
        /// <returns>
        /// Single X509 certificate
        /// </returns>
        public static X509Certificate2 FindCertificate(string certName, StoreName storeName, StoreLocation storeLocation)
        {
            if (SigningCertificate != null)
            {
                return SigningCertificate;
            }

            var store = new X509Store(storeName, storeLocation);
            store.Open(OpenFlags.ReadOnly);

            var cert = store.Certificates.Find(X509FindType.FindByThumbprint, certName, false)
                .Cast<X509Certificate2>().FirstOrDefault();

            return cert;
        }

        /// <summary>
        /// Creates a SAML 2.0 attribute
        /// </summary>
        /// <param name="name">The attribute name</param>
        /// <param name="value">The attribute value</param>
        /// <param name="uri">The name format URI</param>
        /// <returns>SAML 2.0 attribute</returns>
        private static Saml2Attribute CreateSamlAttribute(string name, string value, string uri)
        {
            var attribute = new Saml2Attribute(name);
            attribute.Values.Add(value);
            attribute.NameFormat = new Uri(uri);

            return attribute;
        }
    }
}
