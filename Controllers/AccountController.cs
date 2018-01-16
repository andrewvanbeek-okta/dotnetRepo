using ComponentSpace.Saml2;
using ExampleIdentityProvider.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace ExampleIdentityProvider.Controllers
{
    [Route("[controller]/[action]")]
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger _logger;
        private readonly ISamlIdentityProvider _samlIdentityProvider;

        public AccountController(
            SignInManager<ApplicationUser> signInManager, 
            ILogger<AccountController> logger,
            ISamlIdentityProvider samlIdentityProvider)
        {
            _signInManager = signInManager;
            _logger = logger;
            _samlIdentityProvider = samlIdentityProvider;
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");

            var ssoState = await _samlIdentityProvider.GetStatusAsync();

            if (await ssoState.CanSloAsync())
            {
                // Request logout at the service providers.
                await _samlIdentityProvider.InitiateSloAsync();

                return new EmptyResult();
            }

            return RedirectToPage("/Index");
        }
    }
}
