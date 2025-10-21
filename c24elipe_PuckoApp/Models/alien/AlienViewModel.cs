using Microsoft.AspNetCore.Mvc.Rendering;

namespace c24elipe_PuckoApp.Models;

public class AlienViewModel
{
    public List<AlienModel> Aliens { get; set; } = new List<AlienModel>();
        
    public List<SelectListItem> Races { get; set; } = new List<SelectListItem>();

    public CreateAlienViewModel NewAlien { get; set; } = new CreateAlienViewModel();

    public string? SearchedPNR { get; set; }
}