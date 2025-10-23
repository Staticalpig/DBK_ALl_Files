namespace c24elipe_PuckoApp.Models.Raport;

public class EnableRapportModel
{
    public  string RapportDatum { get; set; }
    public  string RapportNr { get; set; }
    public   string RadId { get; set; }
    public  string RadText { get; set; }

    public bool EnableRadEditing { get; set; } = false;
}