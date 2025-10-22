using Microsoft.AspNetCore.Mvc;

namespace c24elipe_PuckoApp.Controllers;

public class RaportsController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View();
    }
}