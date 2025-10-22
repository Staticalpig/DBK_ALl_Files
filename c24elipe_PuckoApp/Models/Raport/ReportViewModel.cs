namespace c24elipe_PuckoApp.Models.Raport;

public class ReportViewModel
{
    public DateTime Datum { get; set; }
    public int Nr { get; set; }
    public string RapportTyp { get; set; } = string.Empty;
    public string IncidentNamn { get; set; } = string.Empty;
    public string Användarnamn { get; set; } = string.Empty;
    public string Incidentledare { get; set; } = string.Empty;
}