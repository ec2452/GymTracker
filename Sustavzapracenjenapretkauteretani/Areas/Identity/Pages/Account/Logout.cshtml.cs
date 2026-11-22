using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Teretana.Models;

namespace Sustavzapracenjenapretkauteretani.Areas.Identity.Pages.Account
{
    [Authorize]
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(SignInManager<AppUser> signInManager, ILogger<LogoutModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        public async Task<IActionResult> OnPost(string? returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Korisnik se odlogirao.");
            
            if (returnUrl != null)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                return RedirectToPage("../Index", new { area = "" });
            }
        }
    }
}
