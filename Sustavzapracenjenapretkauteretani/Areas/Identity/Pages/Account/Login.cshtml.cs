using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Teretana.Models;

namespace Sustavzapracenjenapretkauteretani.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public IList<AuthenticationScheme> ExternalLogins { get; set; } = default!;

        public string? ReturnUrl { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Lozinka")]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Zapamti me")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string? returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");
            ReturnUrl = returnUrl;

            // If the user is already logged in then redirect back to the home page.
            if (User?.Identity?.IsAuthenticated ?? false)
            {
                Response.Redirect(returnUrl);
                return;
            }

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set lockoutOnFailure: true
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation($"Korisnik {Input.Email} je uspješno prijavljen.");
                    return LocalRedirect(returnUrl);
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning($"Korisnički račun {Input.Email} je zaključan.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Neispravna prijava (email ili lozinka).");
                    _logger.LogWarning($"Neuspješna prijava za {Input.Email}.");
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
