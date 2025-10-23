using System.Runtime.CompilerServices;

namespace c24elipe_PuckoApp.Models.Raport;

public class ViewRapportModel
{
    public List<RapportDBModel> Reports { get; set; } = new List<RapportDBModel>();

    public RapportDBModel SelectedReport { get; set; } = new RapportDBModel();

    public List<RapportDBRadderModel> SelectedRadder { get; set; } = new List<RapportDBRadderModel>();
    
    public List<RapportDBCommentModel> SelectedComments { get; set; } = new List<RapportDBCommentModel>();
    
}