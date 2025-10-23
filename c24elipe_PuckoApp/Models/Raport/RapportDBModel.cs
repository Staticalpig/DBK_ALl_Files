using System.ComponentModel.DataAnnotations;

namespace c24elipe_PuckoApp.Models.Raport;

public class RapportDBModel
{

    [Required]
    public DateTime Datum { get; set; }
    [Required]
    public int Nr { get; set; }
    
    public string RapportTyp { get; set; } = string.Empty;
    
    [Required]
    public int Agent_ID { get; set; }
    public string Användarnamn { get; set; } = string.Empty;
    
    public string Incidentledare { get; set; } = string.Empty;
    
    public DateTime Slutdatum { get; set; }
    
    [Required]
    public string Incident_Namn { get; set; } = string.Empty;
    [Required]
    public int Incident_NR { get; set; }
}