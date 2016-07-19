using CarrierPortal.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web.Mvc;

namespace CarrierPortal.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// The certificate thumb print
        /// </summary>
        private const string CertId = "e835f665259341719edac5e03e4ee893d49d81a1";

        /// <summary>
        /// Action method for index page
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            //Retrieve the base 64 encoded SAML Response
            var samlResponse = this.Request["SAMLResponse"];

            //Return a 400 error if no SAML
            if (samlResponse == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //Decode the saml response from base 64 so we can read it
            var bytes = Convert.FromBase64String(samlResponse);
            samlResponse = Encoding.UTF8.GetString(bytes);

            //Validate the signature on the SAML and read in the assertions
            var assertions = TokenHandler.Read(samlResponse);

            //Create the model
            var model = new ViewModel()
            {
                BillingFirstName = assertions["firstName"],
                BillingMiddleInitial = assertions["middleName"],
                BillingLastName = assertions["lastName"],
                Address1 = assertions["address1"],
                Address2 = assertions["address2"],
                City = assertions["city"],
                State = assertions["state"],
                Zip = assertions["zip"],
                Email = assertions["email"],
                PaymentAmount = assertions["amountOwed"],
                CreditCardNumber = "4111111111111111",
                Cvc = "123",
                ExpirationMonth = 1,
                ExpirationYear = 2018,
                RedirectUrl = assertions["redirectUrl"]
            };

            return View(model);
        }

        /// <summary>
        /// Action method for exchange redirect page
        /// </summary>
        /// <param name="model">View model</param>
        /// <returns>Action Result</returns>
        public ActionResult ExchangeRedirect(ViewModel model)
        {
            //Create the SAML assertions
            var assertions = new Dictionary<string, string>
            {
                {"Payment Transaction ID", Guid.NewGuid().ToString()},
                {"Total Amount Paid", model.PaymentAmount},
                {"Payment Status", "Success"}
            };

            //Create the SAML2.0 protocol response
            var samlResponse = TokenHandler.Write(assertions, "Carrier", "test", CertId, StoreName.My, StoreLocation.LocalMachine);

            //Encode it into base 64
            var bytes = Encoding.UTF8.GetBytes(samlResponse);
            var encodedSamlResponse = Convert.ToBase64String(bytes);

            //Create the view model
            var redirectModel = new ExchangeRedirectViewModel
            {
                PostAction = model.RedirectUrl,
                SamlResponse = encodedSamlResponse
            };

            return this.View(redirectModel);
        }
    }
}