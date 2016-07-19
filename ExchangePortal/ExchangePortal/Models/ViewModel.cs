using System.Collections.Generic;

namespace ExchangePortal.Models
{
    /// <summary>
    /// View Model for home page
    /// </summary>
    public class ViewModel
    {
        /// <summary>
        /// Gets or sets the first name
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the middle intial
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
        /// Gets or sets the billing middle initial
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
        /// Gets or sets address 1
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
    }
}