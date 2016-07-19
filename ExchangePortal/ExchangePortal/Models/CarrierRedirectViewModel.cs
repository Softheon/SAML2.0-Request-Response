
namespace ExchangePortal.Models
{
    /// <summary>
    /// View model for carrier redirect
    /// </summary>
    public class CarrierRedirectViewModel
    {
        /// <summary>
        /// URL to redirect to
        /// </summary>
        public string PostAction { get; set; }

        /// <summary>
        /// Base 64 encoded saml response
        /// </summary>
        public string SamlResponse { get; set; }
    }
}