using System.Diagnostics;
using c24elipe_PuckoApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace c24elipe_PuckoApp.Controllers;

public class AuthController(ILogger<AuthController> logger, IConfiguration config) : Controller
{
    private readonly ILogger<AuthController> _logger = logger;

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

        UserDbCredentialsModel userDbCredentialsModel = new UserDbCredentialsModel(config);
        string hashedPasswordTable = userDbCredentialsModel.GetUserHashedPassword(username);
        
        if (string.IsNullOrEmpty(hashedPasswordTable))
        {
            ViewBag.Message = "Invalid username or password.";
            return View();
        }

        bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, hashedPasswordTable);

        if (!isPasswordValid)
        {
            ViewBag.Message = "Invalid username or password.";
            return View();
        }
        
        List<string> userDetails = userDbCredentialsModel.GetUserDetails(username);
        
        string fornamn = userDetails[0];
        string role = userDetails[1];

        if (fornamn == string.Empty || role == string.Empty)
        {
            ViewBag.Message = "Error retrieving user details.";
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