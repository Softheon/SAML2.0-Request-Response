
namespace CarrierPortal.Models
{
    /// <summary>
    /// Exchange redirect view model
    /// </summary>
    public class ExchangeRedirectViewModel
    {
        /// <summary>
        /// URL to do SAML2.0 HTTP POST
        /// </summary>
        public string PostAction { get; set; }

        /// <summary>
        /// Base 64 encoded SAML response
        /// </summary>
        public string SamlResponse { get; set; }
    }
}