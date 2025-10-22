using System.ComponentModel.DataAnnotations;

namespace c24elipe_PuckoApp.Models;

public class CreateAlienViewModel
{
    [Required]
    public string PNR { get; set; }

    [Required]
    public string Ras { get; set; }

    public string? Hemplanet { get; set; }
    public string? KändaNamn { get; set; }

    [Required]
    public bool ÄrRegistrerad { get; set; } = false; 

    [Required]
    public string Farlighet { get; set; } = "Neutral"; 
}