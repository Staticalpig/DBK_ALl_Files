using Dapper;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using PLEASE_CONNECT.Models;

namespace PLEASE_CONNECT.Controllers;

public class UserController : Controller
{
    private readonly IConfiguration _configuration;

    // Inject IConfiguration to read the connection string
    public UserController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // This action handles requests to /Alien or /Alien/Index
    public IActionResult Index()
    {
        // Create an empty list to hold the results
        IEnumerable<User> users = new List<User>();
            
        try
        {
            // Get the connection string from appsettings.json
            var connectionString = _configuration.GetConnectionString("Login");

            // Use a 'using' block to ensure the connection is closed
            using (var connection = new MySqlConnection(connectionString))
            {
                // SQL query to select all data
                string sql = "SELECT Användarnamn, Lösenord FROM Agent_Open";

                // Use Dapper's .Query<T> method to execute the query 
                // and automatically map the results to a List<Alien>
                users = connection.Query<User>(sql);
            }
        }
        catch (Exception ex)
        {
            // Optional: Log the error or pass it to the view
            ViewData["Error"] = ex.Message;
        }

        // Pass the list of aliens to the View
        return View(users);
    }
}