using c24elipe_PuckoApp.Models.Raport;
using c24elipe_PuckoApp.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace c24elipe_PuckoApp.Controllers;

public class RapportController : Controller
{
    
    private readonly IConfiguration _configuration;
    private readonly ILogger<RapportController> _logger;
    
    
    public RapportController(ILogger<RapportController> logger, IConfiguration configuration)
    {
        this._logger = logger;
        this._configuration = configuration;
    }
    
    
    private List<RapportDBRadderModel> GetRapportDBRadder(DateTime datum, string nr, RapportRepository repository)
    {
        Task<List<RapportDBRadderModel>> task_radder = Task.FromResult<List<RapportDBRadderModel>?>(null);
        try
        {
            task_radder = repository.GetRappportRadder(datum, nr);
        }
        catch (Exception e)
        {
            TempData["ErrorMessage"] = e.Message;
            _logger.LogError(e, "Error fetching rapport radder: {Message}", e.Message);
            return new List<RapportDBRadderModel>();
        }
        task_radder.Wait();
        var radder = task_radder.Result;
        _logger.LogInformation("Fetched {Count} rapport radder from the database.", radder.Count);
        return radder ?? new List<RapportDBRadderModel>();
    }
    
    private List<RapportDBCommentModel> GetRapportDBComments(DateTime datum, string nr, RapportRepository repository)
    {
        Task<List<RapportDBCommentModel>> task_comments = Task.FromResult<List<RapportDBCommentModel>?>(null);
        try
        {
            task_comments = repository.GetRapportComments(datum, nr);
        }
        catch (Exception e)
        {
            TempData["ErrorMessage"] = e.Message;
            _logger.LogError(e, "Error fetching rapport comments: {Message}", e.Message);
            return new List<RapportDBCommentModel>();
        }
        task_comments.Wait();
        var comments = task_comments.Result;
        _logger.LogInformation("Fetched {Count} rapport comments from the database.", comments.Count);
        return comments ?? new List<RapportDBCommentModel>();
    }
    
    [Route("Rapport/AddNewRad")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddNewRad(string RapportDatum, string RapportNr, string RapportRaddText)
    {
        
        _logger.LogInformation("####  AddNewRad called with RapportDatum: {RapportDatum}, RapportNr: {RapportNr}, RapportRaddText: {RapportRaddText}", RapportDatum, RapportNr, RapportRaddText);
        
        string userRole = HttpContext.Session.GetString("Role") ?? "agent";
        var repository = new RapportRepository(_configuration, userRole);
        

        try
        {
            _logger.LogInformation("Creating new rapport comment...");
            await repository.CreateNewRad(RapportDatum,int.Parse(RapportNr), RapportRaddText, userRole);
        }
        catch (Exception e)
        {
            TempData["ErrorMessage"] = e.Message;
            _logger.LogError(e, "!!!!!! Error creating new rapport comment: {Message}", e.Message);
        }

        return RedirectToAction("Index", "Rapport", new { RapportNamn = RapportDatum, RapportNR = RapportNr });
    }
    
    // GET
    public IActionResult Index()
    {
        string userRole = HttpContext.Session.GetString("Role") ?? "agent";
        RapportRepository repository = new RapportRepository(_configuration,userRole);


        var task_rapports = Task.FromResult<List<RapportDBModel>?>(null);
        try
        {
            task_rapports = repository.GetAllRapports(HttpContext.Session.GetString("Username") ?? "");
        }
        catch (Exception e)
        {
            TempData["ErrorMessage"] = e.Message;
            _logger.LogError(e, "Error fetching rapports: {Message}", e.Message);
            
            return View(new ViewRapportModel { Reports = new List<RapportDBModel>() });
        }
        task_rapports.Wait();
        var rapports = task_rapports.Result;
        ViewRapportModel viewModel = new ViewRapportModel
        {
            Reports = rapports ?? new List<RapportDBModel>()
        };
        
        _logger.LogInformation("Fetched {Count} rapports from the database.", rapports.Count);

        var RapportNamn = Request.Query["RapportNamn"].ToString();
        var RapportNr = Request.Query["RapportNR"].ToString();

        _logger.LogInformation("Received query parameters RapportNamn: {RapportNamn}, RapportNr: {RapportNr}.", RapportNamn, RapportNr);

        if (string.IsNullOrEmpty(RapportNamn) || string.IsNullOrEmpty(RapportNr))
        {
            _logger.LogInformation("No specific rapport selected, displaying list only.");
            return View(viewModel);
        }

        if (!DateTime.TryParse(RapportNamn, null, System.Globalization.DateTimeStyles.RoundtripKind, out var parsedDate))
        {
            TempData["ErrorMessage"] = "Invalid date format.";
            return View(viewModel);
        }

        var selectedRaport = viewModel.Reports.FirstOrDefault(r => r.Datum.ToString("o") == parsedDate.ToString("o"));



        _logger.LogInformation("Selected rapport with Datum: {Datum} and Nr: {Nr}.", RapportNamn, RapportNr);
        _logger.LogInformation("---> Datum type: {DatumType}, Nr type: {NrType}.", selectedRaport?.Datum.GetType(), selectedRaport?.Nr.GetType());
        
        if (selectedRaport == null)
        {
            TempData["ErrorMessage"] = "Selected rapport not found.";
            return View(viewModel);
        }
        
        viewModel.SelectedReport = selectedRaport;
        viewModel.SelectedRadder = GetRapportDBRadder(selectedRaport.Datum, selectedRaport.Nr.ToString(), repository);
        viewModel.SelectedComments  = GetRapportDBComments(selectedRaport.Datum, selectedRaport.Nr.ToString(), repository);
        

        return View(viewModel);
    }
}