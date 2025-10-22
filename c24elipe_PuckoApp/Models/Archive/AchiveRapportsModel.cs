using System.ComponentModel.DataAnnotations;

namespace c24elipe_PuckoApp.Models.Archive;

public class AchiveRapportModel
{
    [Required]
    public DateTime Datum { get; set; }
    [Required]
    public uint Nr { get; set; }
    [Required]
    public string RapportTyp { get; set; } = string.Empty;
    [Required]
    public int Agent_ID { get; set; }
    [Required]
    public string Anvandarnamn { get; set; } = string.Empty;
    [Required]
    public string Incidentledare { get; set; } = string.Empty;
    [Required]
    public DateTime Slutdatum { get; set; }
    [Required]
    public string Incident_Namn { get; set; } = string.Empty;
    [Required]
    public uint Incident_NR { get; set; }
    [Required]
    public DateTime ArkivDatum { get; set; }
    [Required]
    public string ArkivAnledning { get; set; } = string.Empty;
    public int? ArkivAv { get; set; }
}