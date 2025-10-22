using c24elipe_PuckoApp.Models.Incident;
using Microsoft.AspNetCore.Mvc;

namespace c24elipe_PuckoApp.Controllers;

public class IncidentController : Controller
{
    private readonly ILogger<IncidentController> logger;
    private readonly IConfiguration _config;
    private IncidentDBModel incidentDbModel;
    private readonly string[] _allowedRoles = { "agent", "group_leader", "admin" };
    
    public IncidentController(ILogger<IncidentController> logger, IConfiguration config)
    {  
        this.logger = logger;
        _config = config;
    }
    
    private List<IncidentViewModel> GetAllIncidents()
    {
        var task_incidents=  incidentDbModel.GetAllIncidentsAsync();
        task_incidents.Wait();
        var incidents = task_incidents.Result;

        logger.LogInformation("Fetched {Count} incidents from the database.", incidents.Count);
        return incidents;
    }   
    
    
    
    private void makeSureDBmodelIsSet()
    {
        if (incidentDbModel != null) return;
        string userRole = HttpContext.Session.GetString("Role") ?? "agent";
        incidentDbModel = new IncidentDBModel(_config, userRole);
    }
    
    [HttpPost]
    public IActionResult CreateIncident(ViewCreateIncidentModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Invalid incident data.";
            logger.LogWarning("Invalid incident data received: {@Name}, {@Nr}, {@Sakerhetsgrad}", model.Namn, model.NR, model.Sakerhetsgrad);
            // move them to Index with tempdata
            return RedirectToAction("Index", "Incident");
        }
        
        logger.LogInformation("Trying to create incident: {@Name}, {@Nr}, {@Sakerhetsgrad}", model.Namn, model.NR, model.Sakerhetsgrad);
        makeSureDBmodelIsSet();
        
        try
        {
            incidentDbModel.CreateIncidentAsync(model.Namn,model.NR,model.Sakerhetsgrad).Wait();
            TempData["SuccessMessage"] = "Incident created successfully.";
            logger.LogInformation("Incident {Name} created successfully.", model.Namn);
        }
        catch (Exception e)
        {
            TempData["ErrorMessage"] = "An error occurred while creating the incident.";
            logger.LogError(e, "Error creating incident {Name}: {Message}", model.Namn, e.Message);
        }
        return RedirectToAction("Index", "Incident");
    }
    
    [HttpPost]
    public IActionResult AddAlienToIncident(string incident,  string alienPnr)
    {
        makeSureDBmodelIsSet();
        
        if (string.IsNullOrEmpty(incident) || string.IsNullOrEmpty(alienPnr))
        {
            TempData["ErrorMessage"] = "Incident name or Alien ID code is missing.";
            logger.LogWarning("Missing Incident name or Alien ID code: {@Incident}, {@AlienPNR}", incident, alienPnr);
            return RedirectToAction("Index", "Incident");
        }
        
        var explodedIncident = incident.Split(" - ");
        if (explodedIncident.Length < 1)
        {
            TempData["ErrorMessage"] = "Invalid Incident format.";
            logger.LogWarning("Invalid Incident format: {@Incident}", incident);
            return RedirectToAction("Index", "Incident");
        }
        
        var incidentNr = explodedIncident[^1]; 
        var incidentNamn = string.Join(" - ", explodedIncident[..^1]);
        
        
        logger.LogInformation("Trying to add Alien '{AlienIdKod}' to Incident '{IncidentNamn}' ('{IncidentNr}').", alienPnr, incidentNamn, incidentNr);
        
        try
        {
            logger.LogInformation("Alien {AlienIdKod} added to incident {IncidentNamn} successfully.", alienPnr, incidentNamn);
            incidentDbModel.AddAlienToIncidentAsync(incidentNamn, incidentNr, alienPnr).Wait();
            TempData["SuccessMessage"] = "Alien added to incident successfully.";
        }
        catch (Exception e)
        {
            TempData["ErrorMessage"] = "An error occurred while adding the alien to the incident.";
            logger.LogError(e, "Error adding alien {AlienIdKod} to incident {IncidentNamn}: {Message}", alienPnr, incidentNamn, e.Message);
        }
        
        return RedirectToAction("Index", "Incident");
    }
    
    public IActionResult Index()
    {
        string userRole = HttpContext.Session.GetString("Role") ?? "agent";
        
        if (!_allowedRoles.Contains(userRole))
        {
            return StatusCode(403, "Access denied. You do not have permission to view this page.");
        }
        
        logger.LogInformation("User role from session: {Role}", userRole);
        makeSureDBmodelIsSet();
        
        List<IncidentViewModel> incidents = GetAllIncidents();
        
        return View(incidents);
    }
    
}