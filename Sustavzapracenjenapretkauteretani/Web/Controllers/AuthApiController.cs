using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sustavzapracenjenapretkauteretani.DAL;
using Sustavzapracenjenapretkauteretani.Web.Api;
using Teretana.Models;

namespace Sustavzapracenjenapretkauteretani.Web.Controllers;

[Route("api/auth")]
[ApiController]
public class AuthApiController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly JwtTokenService _jwtTokenService;
    private readonly ILogger<AuthApiController> _logger;

    public AuthApiController(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        JwtTokenService jwtTokenService,
        ILogger<AuthApiController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning($"Neuspješna prijava: korisnik {request.Email} ne postoji.");
                return Unauthorized(new { message = "Neispravni kredencijali." });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
            {
                _logger.LogWarning($"Neuspješna prijava: pogrešna lozinka za {request.Email}.");
                return Unauthorized(new { message = "Neispravni kredencijali." });
            }

            // Generiraj JWT token
            var roles = await _userManager.GetRolesAsync(user);
            var token = _jwtTokenService.GenerateToken(user, roles);

            _logger.LogInformation($"Korisnik {request.Email} je uspješno prijavljen (API).");

            return Ok(new LoginResponse
            {
                Success = true,
                Token = token,
                User = new UserInfoDTO
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    UserName = user.UserName ?? string.Empty,
                    Roles = roles.ToList()
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška pri prijavljivanju.");
            return StatusCode(500, new { message = "Greška na serveru." });
        }
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            // Provjeri je li korisnik već registriran
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "Korisnik sa ovim emailom već postoji." });
            }

            var newUser = new AppUser
            {
                UserName = request.Email,
                Email = request.Email,
                OIB = request.OIB,
                JMBG = request.JMBG,
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(newUser, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new { message = "Registracija nije uspješna.", errors });
            }

            // Dodaj u "User" ulogu po zadanom
            await _userManager.AddToRoleAsync(newUser, "User");

            _logger.LogInformation($"Novi korisnik registriran: {request.Email}.");

            return Ok(new { message = "Registracija je uspješna. Provjeri email za potvrdu ili se odmah prijavi." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Greška pri registraciji.");
            return StatusCode(500, new { message = "Greška na serveru." });
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string OIB { get; set; } = string.Empty;
        public string JMBG { get; set; } = string.Empty;
    }

    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Token { get; set; } = string.Empty;
        public UserInfoDTO? User { get; set; }
    }
}
