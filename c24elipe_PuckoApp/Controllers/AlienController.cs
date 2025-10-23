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
    public async Task<IActionResult> Index(string pnr) 
    {
        if (!IsAuthorized(out string? role))
        {

            TempData["ErrorMessage"] = "Du måste logga in med rätt behörighet.";
            return RedirectToAction("Index", "Home");
        }

        try
        {
            AlienDbModel alienDbModel = new AlienDbModel(_config, role!);
            
            var viewModel = new AlienViewModel
            {
                Aliens = await alienDbModel.GetAllAliensAsync(),
                

                Races = (role != "agent") ? await alienDbModel.GetAlienRacesAsync() : new(),
                
                SearchedPNR = pnr
            };

            return View(viewModel);
        }
        catch (Exception e)
        {
          
            return StatusCode(500, $"Connection error: {e.Message}");
        }
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AlienViewModel model)
    {
        if (!IsAuthorized(out string? role))
        {
            return RedirectToAction("Index", "Home");
        }
        
        if (role == "agent")
        {
            return Forbid(); 
        }
        
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
        
        
        return RedirectToAction("Index");
    }
}
