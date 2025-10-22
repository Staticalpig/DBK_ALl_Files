using Microsoft.AspNetCore.Mvc;

namespace c24elipe_PuckoApp.Controllers;

public class ArchiveController(IConfiguration config) : Controller
{
    private readonly IConfiguration _config = config;
    private readonly string[] _allowedRoles = { "admin" };

    // Helper to check session and role. Agents are NOT allowed.
    private bool IsAuthorized(out string? role)
    {
        role = HttpContext.Session.GetString("Role");
        return HttpContext.Session.GetString("IsLoggedIn") == "true" &&
               role != null && _allowedRoles.Contains(role);
    }

    [HttpGet]
    public IActionResult ArchiveRapport()
    {
        if (!IsAuthorized(out var role))
        {
            TempData["ErrorMessage"] = "You must be logged in as an admin to archive rapports.";
            return RedirectToAction("Index", "Home");
        }

        try
        {
            var archiveDbModel = new Models.Archive.ArchiveDBModel(_config, role);
            archiveDbModel.ArchiveAlienAsync().Wait();

            TempData["SuccessMessage"] = "Old rapports have been successfully archived.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "An error occurred while archiving rapports: " + ex.Message;
        }

        return RedirectToAction("Index", "Archive");
    }
    
    [HttpGet]
    public IActionResult ArchiveAlien()
    {
        if (!IsAuthorized(out var role))
        {
            TempData["ErrorMessage"] = "You must be logged in as an admin to archive aliens.";
            return RedirectToAction("Index", "Home");
        }

        try
        {
            var archiveDbModel = new Models.Archive.ArchiveDBModel(_config, role);
            archiveDbModel.ArchiveAlienAsync().Wait();

            TempData["SuccessMessage"] = "Old aliens have been successfully archived.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "An error occurred while archiving aliens: " + ex.Message;
        }

        return RedirectToAction("Index", "Archive");
    }
    
    [HttpGet]
    public IActionResult Index()
    {
        if (!IsAuthorized(out var role))
        {
            TempData["ErrorMessage"] = "You must be logged in as an admin to access the archive.";
            return RedirectToAction("Index", "Home");
        }
        
        var connectionString = _config.GetConnectionString("DefaultConnection")!;
        
        return View();
    }
}