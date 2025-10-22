using System.ComponentModel.DataAnnotations;
using Google.Protobuf.WellKnownTypes;

namespace c24elipe_PuckoApp.Models.Incident;

public class AlienDataDbModel
{
    
    public string Id { get; set; } =  string.Empty;
    
    [Required]
    public string PNR { get; set; }
    
    [Required]
    public string Alias { get; set; }
}