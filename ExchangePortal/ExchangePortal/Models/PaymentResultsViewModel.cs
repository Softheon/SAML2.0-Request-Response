
namespace ExchangePortal.Models
{
    /// <summary>
    /// Payment Results View Model
    /// </summary>
    public class PaymentResultsViewModel
    {
        /// <summary>
        /// Gets or sets the Payment Transaction ID
        /// </summary>
        public string PaymentTransactionId { get; set; }

        /// <summary>
        /// Gets or sets the Payment Amount
        /// </summary>
        public decimal PaymentAmount { get; set; }

        /// <summary>
        /// Gets or sets the payment status
        /// </summary>
        public string PaymentStatus { get; set; }
    }
}