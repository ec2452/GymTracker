using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Teretana.Models;

namespace Sustavzapracenjenapretkauteretani.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserStore<AppUser> _userStore;
        private readonly IUserEmailStore<AppUser> _emailStore;
        private readonly ILogger<ExternalLoginModel> _logger;

        public ExternalLoginModel(
            SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager,
            IUserStore<AppUser> userStore,
            ILogger<ExternalLoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager   = userManager;
            _userStore     = userStore;
            _emailStore    = GetEmailStore();
            _logger        = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public string ProviderDisplayName { get; set; } = default!;

        public string ReturnUrl { get; set; } = default!;

        [TempData]
        public string ErrorMessage { get; set; } = default!;

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required]
            [StringLength(11, MinimumLength = 11)]
            [RegularExpression("^[0-9]*$", ErrorMessage = "OIB smije sadržavati samo brojeve.")]
            [Display(Name = "OIB")]
            public string OIB { get; set; } = string.Empty;

            [Required]
            [StringLength(13, MinimumLength = 13)]
            [RegularExpression("^[0-9]*$", ErrorMessage = "JMBG smije sadržavati samo brojeve.")]
            [Display(Name = "JMBG")]
            public string JMBG { get; set; } = string.Empty;
        }

        public IActionResult OnGet() => RedirectToPage("./Login");

        public IActionResult OnPost(string provider, string? returnUrl = null)
        {
            var redirectUrl = Url.Page("./ExternalLogin", pageHandler: "Callback",
                values: new { returnUrl });
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(
                provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        public async Task<IActionResult> OnGetCallbackAsync(string? returnUrl = null, string? remoteError = null)
        {
            returnUrl ??= Url.Content("~/");

            if (remoteError != null)
            {
                ErrorMessage = $"Greška od vanjskog providera: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Greška pri dohvatu informacija o vanjskoj prijavi.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            var result = await _signInManager.ExternalLoginSignInAsync(
                info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("{Name} prijavljen vanjskim providerom {LoginProvider}.",
                    info.Principal.Identity?.Name, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }

            if (result.IsLockedOut)
                return RedirectToPage("./Lockout");

            ReturnUrl           = returnUrl;
            ProviderDisplayName = info.ProviderDisplayName ?? info.LoginProvider;

            if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                Input = new InputModel
                {
                    Email = info.Principal.FindFirstValue(ClaimTypes.Email) ?? string.Empty
                };

            return Page();
        }

        public async Task<IActionResult> OnPostConfirmationAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ErrorMessage = "Greška pri dohvatu informacija o vanjskoj prijavi.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {
                var user = new AppUser
                {
                    UserName = Input.Email,
                    Email    = Input.Email,
                    OIB      = Input.OIB,
                    JMBG     = Input.JMBG
                };

                await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                var result = await _userManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await _userManager.AddLoginAsync(user, info);
                    if (result.Succeeded)
                    {
                        _logger.LogInformation(
                            "Korisnik kreirao račun koristeći {Name} provider.", info.LoginProvider);
                        await _signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
                        return LocalRedirect(returnUrl);
                    }
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }

            ProviderDisplayName = info.ProviderDisplayName ?? info.LoginProvider;
            ReturnUrl           = returnUrl;
            return Page();
        }

        private IUserEmailStore<AppUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
                throw new NotSupportedException("Zadano UI zahtijeva korisnički store s podrškom za email.");
            return (IUserEmailStore<AppUser>)_userStore;
        }
    }
}
