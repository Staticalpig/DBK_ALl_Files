using Microsoft.AspNetCore.Mvc;

namespace c24elipe_PuckoApp.Controllers;

public class RaportsController : Controller
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<RaportsController> _logger;
    
    public RaportsController( IConfiguration conf, ILogger<RaportsController> logger)
    {
        _configuration = conf;
        _logger = logger;
        
    }
    // GET
    public IActionResult Index()
    {
        return View();
    }
}