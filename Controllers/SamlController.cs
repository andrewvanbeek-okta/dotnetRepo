using ComponentSpace.Saml2;
using ComponentSpace.Saml2.Assertions;
using ExampleIdentityProvider.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ExampleIdentityProvider.Controllers
{
    public class SamlController : Controller
    {
        private readonly ISamlIdentityProvider _samlIdentityProvider;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public SamlController(ISamlIdentityProvider samlIdentityProvider, SignInManager<ApplicationUser> signInManager)
        {
            _samlIdentityProvider = samlIdentityProvider;
            _signInManager = signInManager;
        }

        public async Task<ActionResult> SingleSignOnService()
        {
            // Receive the authn request from the service provider (SP-initiated SSO).
            await _samlIdentityProvider.ReceiveSsoAsync();

            // If the user isn't logged in at the identity provider, 
            // have the user login before completing SSO.
            return RedirectToAction("SingleSignOnServiceCompletion");
        }

        [Authorize]
        public async Task<ActionResult> SingleSignOnServiceCompletion()
        {
            // Get the name of the logged in user.
            var userName = User.Identity.Name;

            // For demonstration purposes, include some claims.
            var attributes = new List<SamlAttribute>()
            {
                new SamlAttribute(ClaimTypes.GivenName, ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.GivenName).Value),
                new SamlAttribute(ClaimTypes.Surname, ((ClaimsIdentity)User.Identity).FindFirst(ClaimTypes.Surname).Value)
            };

            // The user is logged in at the identity provider.
            // Respond to the authn request by sending a SAML response containing a SAML assertion to the SP.
            await _samlIdentityProvider.SendSsoAsync(userName, attributes);

            return new EmptyResult();
        }

        public async Task<ActionResult> SingleLogoutService()
        {
            // Receive the single logout request or response.
            // If a request is received then single logout is being initiated by a partner service provider.
            // If a response is received then this is in response to single logout having been initiated by the identity provider.
            var sloResult = await _samlIdentityProvider.ReceiveSloAsync();

            if (sloResult.IsResponse)
            {
                if (sloResult.HasCompleted)
                {
                    // IdP-initiated SLO has completed.
                    return RedirectToPage("/Index");
                }
            }
            else
            {
                // Logout locally.
                await _signInManager.SignOutAsync();

                // Respond to the SP-initiated SLO request indicating successful logout.
                await _samlIdentityProvider.SendSloAsync();
            }

            return new EmptyResult();
        }

        public async Task<ActionResult> ArtifactResolutionService()
        {
            // Resolve the HTTP artifact.
            // This is only required if supporting the HTTP-Artifact binding.
            await _samlIdentityProvider.ResolveArtifactAsync();

            return new EmptyResult();
        }
    }
}