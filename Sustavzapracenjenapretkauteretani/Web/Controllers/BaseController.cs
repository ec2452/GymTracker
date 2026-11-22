using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Teretana.Models;

namespace Sustavzapracenjenapretkauteretani.Web.Controllers;

public abstract class BaseController : Controller
{
    protected readonly UserManager<AppUser> UserManager;

    protected BaseController(UserManager<AppUser> userManager)
    {
        UserManager = userManager;
    }

    protected string? UserId => UserManager.GetUserId(User);
}
