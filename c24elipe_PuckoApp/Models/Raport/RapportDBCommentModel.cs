namespace c24elipe_PuckoApp.Models.Raport;

public class RapportDBCommentModel
{
    
    public DateTime Rapport_Datum { get; set; }
    public int Rapport_Nr { get; set; }
    public int Nr { get; set; }
    
    public string Text { get; set; } = string.Empty;
    
    public string? GjordAvNamn { get; set; }
}