using System.ComponentModel.DataAnnotations;

namespace c24elipe_PuckoApp.Models.Incident;

public class ViewCreateIncidentModel
{
    [Required]
    public string Namn { get; set; }
    [Required]
    public int NR { get; set; }
    [Required]
    public string Sakerhetsgrad { get; set; }
}