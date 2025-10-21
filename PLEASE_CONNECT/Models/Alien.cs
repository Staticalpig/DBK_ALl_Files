using System.ComponentModel.DataAnnotations;

namespace PLEASE_CONNECT.Models;

public class Alien
{
    [Display(Name = "ID Code")]
    public string IdKod { get; set; }

    public string PNR { get; set; }
        
    public string Ras { get; set; }

    [Display(Name = "Home Planet")]
    public string Hemplanet { get; set; }

    [Display(Name = "Known Names")]
    public string KändaNamn { get; set; }

    [Display(Name = "Is Registered")]
    public bool ÄrRegistrerad { get; set; }

    [Display(Name = "Danger Level")]
    public string Farlighet { get; set; }
}