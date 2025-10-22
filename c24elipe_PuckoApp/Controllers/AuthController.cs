using System.Diagnostics;
using c24elipe_PuckoApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace c24elipe_PuckoApp.Controllers;

public class AuthController(ILogger<AuthController> logger, IConfiguration config) : Controller
{

    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Index(string username, string password)
    {
        Debug.WriteLine("Attempting login for user: " + username);

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            ViewBag.Message = "Please enter both username and password.";
            return View();
        }
        
        UserDbCredentialsModel userDbCredentialsModel;
        string hashedPasswordTable;   
        
        try
        {
            userDbCredentialsModel = new UserDbCredentialsModel(config);
            hashedPasswordTable = userDbCredentialsModel.GetUserHashedPassword(username);
        }
        
        catch (Exception e)
        {
            TempData["ErrorMessage"] = "An error occurred while processing your request. Please try again later.";
            logger.LogError(e, "Error retrieving hashed password for user: " + username);
            return View();
        }
        
        if (string.IsNullOrEmpty(hashedPasswordTable))
        {
            TempData["ErrorMessage"] = "Invalid username or password.";
            logger.LogWarning("Login failed for non-existent user: " + username);
            return View();
        }

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, hashedPasswordTable);

        if (!isPasswordValid)
        {
            TempData["ErrorMessage"] = "Invalid username or password.";
            logger.LogWarning("Login failed for user: " + username + " due to incorrect password.");
            return View();
        }
        
        List<string> userDetails = userDbCredentialsModel.GetUserDetails(username);
        
        string fornamn = userDetails[0];
        string role = userDetails[1];

        if (fornamn == string.Empty || role == string.Empty)
        {
            TempData["ErrorMessage"] = "Error retrieving user details.";
            logger.LogError("User details incomplete for user: " + username + ". Fornamn or role is empty.");
            
            return View();
        }

        if (role != "admin" && role != "group_leader" && role != "agent")
        {
            role = "agent";
        }
        
        HttpContext.Session.SetString("Username", username);
        HttpContext.Session.SetString("Fornamn", fornamn);
        HttpContext.Session.SetString("Role", role);
        HttpContext.Session.SetString("IsLoggedIn", "true");
        
        return RedirectToAction("Index", "Home");
    }
    
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Auth");
    }
}