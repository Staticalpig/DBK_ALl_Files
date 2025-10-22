using c24elipe_PuckoApp.Models.Incident;

namespace c24elipe_PuckoApp.Models.Incident;

public class IncidentViewModel
{
    public string Namn { get; set; }
    public int NR { get; set; }
    public string Sakerhetsgrad { get; set; }
    
    public Dictionary<string, string> AlienNameToPNRMap { get; set; }
    
    public List<AlienDataDbModel> Aliens { get; set; }
    
    public List<AlienDataDbModel> AllAliens { get; set; }
    
}