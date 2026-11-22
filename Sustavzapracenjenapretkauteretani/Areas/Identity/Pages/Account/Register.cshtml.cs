using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Teretana.Models;

namespace Sustavzapracenjenapretkauteretani.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserStore<AppUser> _userStore;
        private readonly IUserEmailStore<AppUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(
            UserManager<AppUser> userManager,
            IUserStore<AppUser> userStore,
            SignInManager<AppUser> signInManager,
            ILogger<RegisterModel> logger)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public string ReturnUrl { get; set; } = default!;

        public IList<AuthenticationScheme> ExternalLogins { get; set; } = default!;

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required]
            [StringLength(100, ErrorMessage = "Lozinka mora imati najmanje {2} znakova.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Lozinka")]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Potvrda lozinke")]
            [Compare("Password", ErrorMessage = "Lozinka i potvrda se ne podudaraju.")]
            public string ConfirmPassword { get; set; } = string.Empty;

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

        public async Task OnGetAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

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

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Korisnik je kreirao novi račun s lozinkom.");
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }

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
