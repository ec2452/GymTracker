using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Sustavzapracenjenapretkauteretani.DAL.Repositories;

namespace Sustavzapracenjenapretkauteretani.Web.Filters;

public class NavBadgeFilter : IActionFilter
{
    private readonly TreningRepository _treningRepository;
    private readonly VjezbaRepository _vjezbaRepository;
    private readonly PlanRepository _planRepository;
    private readonly CiljRepository _ciljRepository;
    private readonly MjerenjeRepository _mjerenjeRepository;
    private readonly KorisnikRepository _korisnikRepository;

    public NavBadgeFilter(
        TreningRepository treningRepository,
        VjezbaRepository vjezbaRepository,
        PlanRepository planRepository,
        CiljRepository ciljRepository,
        MjerenjeRepository mjerenjeRepository,
        KorisnikRepository korisnikRepository)
    {
        _treningRepository = treningRepository;
        _vjezbaRepository = vjezbaRepository;
        _planRepository = planRepository;
        _ciljRepository = ciljRepository;
        _mjerenjeRepository = mjerenjeRepository;
        _korisnikRepository = korisnikRepository;
    }

    public void OnActionExecuting(ActionExecutingContext context) { }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Controller is not Controller controller)
            return;

        controller.ViewData["TreninziCount"] = _treningRepository.GetAll().Count();
        controller.ViewData["VjezbeCount"] = _vjezbaRepository.GetAll().Count();
        controller.ViewData["PlanoviCount"] = _planRepository.GetAll().Count();
        controller.ViewData["CiljeviCount"] = _ciljRepository.GetAll().Count();
        controller.ViewData["MjerenjaCount"] = _mjerenjeRepository.GetAll().Count();
        controller.ViewData["KorisniciCount"] = _korisnikRepository.GetAll().Count();
    }
}
