using System.Data.Common;
using c24elipe_PuckoApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using MySql.Data.MySqlClient;

namespace c24elipe_PuckoApp.Controllers;

public class AlienController : Controller
{
    private readonly IConfiguration _config;
    private readonly string[] _allowedRoles = { "agent", "group_leader", "admin" };


    public AlienController(IConfiguration config)
    {
        _config = config;
    }

    // Helper to check session and role
    private bool IsAuthorized(out string? role)
    {
        role = HttpContext.Session.GetString("Role");
        if (HttpContext.Session.GetString("IsLoggedIn") != "true" || 
            role == null || 
            !_allowedRoles.Contains(role))
        {
            return false;
        }
        return true;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string pnr) // pnr is for the search/highlight
    {
        if (!IsAuthorized(out string? role))
        {
            // Redirect to login with a message, like the PHP script
            return RedirectToAction("Index", "Home", new { 
                redirected = true, 
                reason = "Du måste logga in med rätt behörighet." 
            });
        }

        try
        {
            AlienDbModel alienDbModel = new AlienDbModel(_config, role!);
            
            // Use the main ViewModel
            var viewModel = new AlienViewModel
            {
                // Get all aliens
                Aliens = await alienDbModel.GetAllAliensAsync(),
                
                // Get races for the dropdown (only if user is not an agent)
                Races = (role != "agent") ? await alienDbModel.GetAlienRacesAsync() : new(),
                
                // Pass the searched PNR to the view for highlighting
                SearchedPNR = pnr 
            };

            return View(viewModel);
        }
        catch (Exception e)
        {
            // Handle DB connection errors or other issues
            return StatusCode(500, $"Connection error: {e.Message}");
        }
    }
    
    // This action replaces SearchByPNR and handles the POST for adding an alien
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AlienViewModel model)
    {
        if (!IsAuthorized(out string? role))
        {
            return RedirectToAction("Index", "Home");
        }

        // Role check: Only group_leader and admin can add aliens
        if (role == "agent")
        {
            return Forbid(); // 403 Forbidden
        }
        
        // Bind the form data from the main view model
        var newAlien = model.NewAlien; 

        if (ModelState.IsValid)
        {
            try
            {
                AlienDbModel alienDbModel = new AlienDbModel(_config, role!);
                await alienDbModel.CreateAlienAsync(newAlien);
                TempData["SuccessMessage"] = "Alien skapad!";
            }
            catch (Exception e)
            {
                TempData["ErrorMessage"] = $" {e.Message}";
            }
        }
        else
        {
            TempData["ErrorMessage"] = "Formuläret är felaktigt ifyllt.";
        }

        // Post-Redirect-Get pattern: Redirect back to the Index action
        return RedirectToAction("Index");
    }
}
