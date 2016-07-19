using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web.Mvc;
using ExchangePortal.Models;

namespace ExchangePortal.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// The certificate ID
        /// </summary>
        private const string CertId = "e835f665259341719edac5e03e4ee893d49d81a1";

        /// <summary>
        /// Action method for Index page
        /// </summary>
        /// <returns>Action Result</returns>
        public ActionResult Index()
        {
            //Create the view model for the index page. Hard coded with dummy data for demo purposes
            var model = new ViewModel
            {
                FirstName = "John",
                MiddleInitial = "D",
                LastName = "Doe",
                BillingFirstName = "John",
                BillingMiddleInitial = "D",
                BillingLastName = "Doe",
                Email = "johndoe@domain.com",
                Address1 = "123 Main Street",
                Address2 = "Suite A",
                City = "Seattle",
                State = "WA",
                Zip = "12345",
                States = new List<string>{ "AL", "AK", "AZ", "AR", "CA", "CO", "CT", "DE", "FL", "GA", "HI", "ID", "IL", "IN", "IA", "KS", "KY", "LA", "ME", "MD", "MA", "MI", "MN"
                    , "MS", "MO", "MT", "NE", "NV", "NH", "NJ", "NM", "NY", "NC", "ND", "OH", "OK", "OR", "PA", "RI", "SC", "SD", "TN", "TX", "UT", "VT", "VA"
                    , "WA", "WV", "WI", "WY" },
                PaymentAmount = "99.99"
            };

            return this.View(model);
        }

        /// <summary>
        /// Action method for Carrier Redirect Page
        /// </summary>
        /// <param name="model">View Model</param>
        /// <returns>Action Result</returns>
        public ActionResult CarrierRedirect(ViewModel model)
        {
            //Create the SAML assertions
            var assertions = new Dictionary<string, string>
            {
                {"Partner Assigned Consumer ID", "123456789"},
                {"Subscriber Identifier", "123456789"},
                {"User Type", "Consumer"},
                {"Return URL", @"http://localhost/Exchange/Home/Results"},
                {"Payment Transaction ID", "123456789"},
                {"Market Indicator", "Individual"},
                {"Assigned QHP Identifier", "123456789"},
                {"Total Amount Owed", "99.99"},
                {"Premium Amount Total", "99.99"},
                {"APTC Amount", "0.00"},
                {"Proposed Coverage Effective Date", "2017-01-01"},
                {"First Name", model.FirstName},
                {"Middle Name", model.MiddleInitial},
                {"Last Name", model.LastName},
                {"Street Name 1", model.Address1},
                {"Street Name 2", model.Address2},
                {"City", model.City},
                {"State", model.State},
                {"Zip Code", model.Zip},
                {"Contact Email Address", model.Email},
                {"TIN Identification", "111223333"}
            };

            //Create the SAML2.0 protocol xml
            var samlResponse = TokenHandler.Write(assertions, "Exchange", "Test", CertId, StoreName.My, StoreLocation.LocalMachine);

            //Convert it to a byte array and encode to base 64
            var bytes = Encoding.UTF8.GetBytes(samlResponse);
            var encodedSamlResponse = Convert.ToBase64String(bytes);

            //Do an HTTP SAML POST to the carrier
            var redirectModel = new CarrierRedirectViewModel
            {
                PostAction = @"http://localhost/CarrierPortal/",
                SamlResponse = encodedSamlResponse
            };

            return this.View(redirectModel);
        }

        /// <summary>
        /// Action method for Results page
        /// </summary>
        /// <returns>Action Result</returns>
        public ActionResult Results()
        {
            //Retrieve the base 64 encoded SAML Response
            var samlResponse = this.Request["SAMLResponse"];

            //Return a 400 error if no saml
            if (samlResponse == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //Decode the saml response from base 64 so we can read it
            var bytes = Convert.FromBase64String(samlResponse);
            samlResponse = Encoding.UTF8.GetString(bytes);

            //Validate the signature on the SAML and read in the assertions
            var assertions = TokenHandler.Read(samlResponse);

            //Create a model with the payment results
            var model = new PaymentResultsViewModel
            {
                PaymentTransactionId = assertions["paymentTransactionID"],
                PaymentAmount = Convert.ToDecimal(assertions["totalAmountPaid"]),
                PaymentStatus = assertions["paymentStatus"]
            };

            return this.View(model);
        }
    }
}