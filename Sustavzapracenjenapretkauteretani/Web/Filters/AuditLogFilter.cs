using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Diagnostics;

namespace Sustavzapracenjenapretkauteretani.Web.Filters;

/// <summary>
/// Globalni filter koji bilježi svaki HTTP zahtjev kao strukturirani audit log zapis.
/// Logira: metodu, putanju, korisnika, akciju, status i trajanje.
/// </summary>
public class AuditLogFilter : IActionFilter
{
    private readonly ILogger<AuditLogFilter> _logger;

    public AuditLogFilter(ILogger<AuditLogFilter> logger)
    {
        _logger = logger;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        context.HttpContext.Items["_audit_ts"] = Stopwatch.GetTimestamp();
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        var startTs = context.HttpContext.Items.TryGetValue("_audit_ts", out var ts)
            ? (long)ts!
            : Stopwatch.GetTimestamp();

        var elapsedMs = (Stopwatch.GetTimestamp() - startTs) * 1000.0 / Stopwatch.Frequency;

        var req        = context.HttpContext.Request;
        var method     = req.Method;
        var path       = req.Path.Value ?? "/";
        var controller = (context.Controller as Controller)?.ControllerContext
                             .ActionDescriptor.ControllerName ?? "?";
        var action     = (context.Controller as Controller)?.ControllerContext
                             .ActionDescriptor.ActionName ?? "?";
        var userId     = context.HttpContext.User?.Identity?.Name ?? "anonimni";
        var statusCode = (context.Result as ObjectResult)?.StatusCode
                      ?? (context.Result as StatusCodeResult)?.StatusCode
                      ?? (context.Result as ViewResult != null ? 200 : (int?)null)
                      ?? (context.Result as RedirectToActionResult != null ? 302 : (int?)null)
                      ?? 200;

        if (context.Exception is not null)
        {
            _logger.LogError(context.Exception,
                "AUDIT ERROR | {Method} {Path} | {Controller}/{Action} | Korisnik={UserId} | {ElapsedMs:F1}ms",
                method, path, controller, action, userId, elapsedMs);
            return;
        }

        // Mutacije (POST/PUT/DELETE) logiramo kao Information, sve ostalo kao Debug.
        if (method is "POST" or "PUT" or "DELETE" or "PATCH")
        {
            _logger.LogInformation(
                "AUDIT {Method} {Path} | {Controller}/{Action} | Korisnik={UserId} | Status={StatusCode} | {ElapsedMs:F1}ms",
                method, path, controller, action, userId, statusCode, elapsedMs);
        }
        else
        {
            _logger.LogDebug(
                "AUDIT {Method} {Path} | {Controller}/{Action} | Korisnik={UserId} | Status={StatusCode} | {ElapsedMs:F1}ms",
                method, path, controller, action, userId, statusCode, elapsedMs);
        }
    }
}
