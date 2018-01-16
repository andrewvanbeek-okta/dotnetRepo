using ComponentSpace.Saml2;
using ComponentSpace.Saml2.Assertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ExampleIdentityProvider.Controllers
{
    public class HomeController : Controller
    {
        private readonly ISamlIdentityProvider _samlIdentityProvider;
        private readonly IConfiguration _configuration;

        public HomeController(ISamlIdentityProvider samlIdentityProvider, IConfiguration configuration)
        {
            _samlIdentityProvider = samlIdentityProvider;
            _configuration = configuration;
        }

        [Authorize]
        public async Task<ActionResult> SingleSignOn()
        {
            // Get the name of the logged in user.
         var userName = "andrew.vanbeek@okta.com";
            // For demonstration purposes, include some claims.
            var attributes = new List<SamlAttribute>()
            {
                new SamlAttribute(ClaimTypes.GivenName, "avb"),
                new SamlAttribute(ClaimTypes.Surname, "avb")
            };

            // Initiate single sign-on to the service provider (IdP-initiated SSO)
            // by sending a SAML response containing a SAML assertion to the SP.
            // The optional relay state normally specifies the target URL once SSO completes.
            var partnerName = _configuration["PartnerName"];
            var relayState = _configuration["RelayState"];
            System.Diagnostics.Debug.WriteLine("test");

            await _samlIdentityProvider.InitiateSsoAsync(partnerName, userName, attributes, relayState);

             return new EmptyResult();
        }
    }
}