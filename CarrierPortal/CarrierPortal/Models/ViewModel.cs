using System.Collections.Generic;

namespace CarrierPortal.Models
{
    /// <summary>
    /// View model for the main page
    /// </summary>
    public class ViewModel
    {
        /// <summary>
        /// Gets or sets the first name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the middle initial
        /// </summary>
        public string MiddleInitial { get; set; }

        /// <summary>
        /// Gets or sets the last name
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Gets or sets the billing first name
        /// </summary>
        public string BillingFirstName { get; set; }

        /// <summary>
        /// Gets or sets the billing middle intiial
        /// </summary>
        public string BillingMiddleInitial { get; set; }

        /// <summary>
        /// Gets or sets the billing last name
        /// </summary>
        public string BillingLastName { get; set; }

        /// <summary>
        /// Gets or sets the email
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the address 1
        /// </summary>
        public string Address1 { get; set; }

        /// <summary>
        /// Gets or sets the address 2
        /// </summary>
        public string Address2 { get; set; }

        /// <summary>
        /// Gets or sets the city
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Gets or sets the state
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Gets or sets the zip code
        /// </summary>
        public string Zip { get; set; }

        /// <summary>
        /// Gets or sets the payment amount
        /// </summary>
        public string PaymentAmount { get; set; }

        /// <summary>
        /// Gets or sets the states
        /// </summary>
        public IEnumerable<string> States { get; set; }

        /// <summary>
        /// Gets or sets the credit card number
        /// </summary>
        public string CreditCardNumber { get; set; }

        /// <summary>
        /// Gets or sets the CVC
        /// </summary>
        public string Cvc { get; set; }

        /// <summary>
        /// Gets or sets the expiration month
        /// </summary>
        public int ExpirationMonth { get; set; }

        /// <summary>
        /// Gets or sets the expiration year
        /// </summary>
        public int ExpirationYear { get; set; }

        /// <summary>
        /// Gets or sets the redirect URL
        /// </summary>
        public string RedirectUrl { get; set; }
    }
}