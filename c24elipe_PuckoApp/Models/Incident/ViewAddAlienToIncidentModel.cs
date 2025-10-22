using System.ComponentModel.DataAnnotations;

namespace c24elipe_PuckoApp.Models.Incident;

public class ViewAddAlienToIncidentModel
{
    [Required]
    public int IncidentId { get; set; }
    [Required]
    public int IncidentNr { get; set; }
    [Required]
    public string AlienIdKod { get; set; }
}