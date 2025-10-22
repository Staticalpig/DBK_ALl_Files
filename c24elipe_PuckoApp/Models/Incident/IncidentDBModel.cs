using System.Data;
using System.Data.Common;
using MySql.Data.MySqlClient;

namespace c24elipe_PuckoApp.Models.Incident;

public class IncidentDBModel
{
    private IConfiguration _config;
    private readonly string _connectionString;
    private readonly ILogger<IncidentDBModel> _logger;

    public IncidentDBModel(IConfiguration config, string userRole)
    {
        _config = config;
        _logger = LoggerFactory.Create(builder => 
            builder.AddConsole()).CreateLogger<IncidentDBModel>();
        
        userRole = userRole ?? "agent";
        _connectionString = _config[userRole];
        
    }

    public async Task<List<IncidentViewModel>> GetAllIncidentsAsync()
    {
        var incidents = new Dictionary<string, IncidentViewModel>();

        await using var dbConnection = new MySqlConnection(_connectionString);
        await dbConnection.OpenAsync();

        string getIncidentsQuery = @"
            SELECT 
            I.Namn, I.NR, I.Säkerhetsgrad,
            A.PNR, A.KändaNamn
        FROM Incident AS I
        LEFT JOIN Incident_Alien AS IA ON IA.Incident_Namn = I.Namn
        LEFT JOIN Alien AS A ON IA.Alien_IdKod = A.IdKod;";
        
        Dictionary<string, List<string>> IncidentToalienPnr = new Dictionary<string, List<String>>();

        string getAliensQuery = "SELECT IdKod, PNR, KändaNamn FROM Alien;";

        var cmd = new MySqlCommand(getIncidentsQuery, dbConnection);

        await using DbDataReader reader = await cmd.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            string namn = reader.GetString("Namn");
                
            if (!incidents.TryGetValue(namn, out var incident))
            {
                incident = new IncidentViewModel
                {
                    Namn = namn,
                    NR = reader.GetInt32("NR"),
                    Sakerhetsgrad = reader.GetString("Säkerhetsgrad"),
                    Aliens = new List<AlienDataDbModel>()
                };

                if ( !IncidentToalienPnr.ContainsKey(namn))
                {
                    IncidentToalienPnr.Add(namn, new List<string>());
                }
                
                incidents[namn] = incident;
            }

             
            if (reader.IsDBNull(reader.GetOrdinal("PNR"))) continue;
                
            var alien = new AlienDataDbModel
            {
                PNR = reader.GetString("PNR"),
                Alias = reader.IsDBNull(reader.GetOrdinal("KändaNamn"))
                    ? "(Unknown)"
                    : reader.GetString("KändaNamn")
            };

            if (!IncidentToalienPnr[namn].Contains(alien.PNR))
            {
                IncidentToalienPnr[namn].Add(alien.PNR);
            }
            
            incident.Aliens.Add(alien);
        }

        await reader.CloseAsync();
        
        var allAliens = new List<AlienDataDbModel>();

        await using var cmd2 = new MySqlCommand(getAliensQuery, dbConnection);
        await using var alienReader = await cmd2.ExecuteReaderAsync();

        while (await alienReader.ReadAsync())
        {
            var alien = new AlienDataDbModel
            {
                Id = alienReader.GetString("IdKod"),
                PNR = alienReader.GetString("PNR"),
                Alias = alienReader.IsDBNull(alienReader.GetOrdinal("KändaNamn"))
                    ? "(Unknown)"
                    : alienReader.GetString("KändaNamn")
            };
            
            allAliens.Add(alien);
        }

        foreach (var i in incidents.Values)
        {
            i.AllAliens = allAliens;
            
            
        }

        return incidents.Values.ToList();
    }
    
    public async Task AddAlienToIncidentAsync(string incidentName, string incidentNr, string alienIdKod)
    {
        _logger.LogInformation("Adding Alien in model '{AlienIdKod}' to Incident '{IncidentName}' ('{IncidentNr}').", alienIdKod, incidentName, incidentNr);
        await using var dbConnection = new MySqlConnection(_connectionString);
        await dbConnection.OpenAsync();

        /* CALL sp_add_alien_to_incident(@incidentNamn, @incidentNr, @alienIdKod);*/
        
        var cmd = new MySqlCommand(@"CALL sp_add_alien_to_incident(@incidentNamn, @incidentNr, (Select IdKod from Alien where PNR = @alienIdKod));", dbConnection);
        cmd.Parameters.AddWithValue("@incidentNamn", incidentName);
        cmd.Parameters.AddWithValue("@incidentNr", incidentNr);
        cmd.Parameters.AddWithValue("@alienIdKod", alienIdKod);
        
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task CreateIncidentAsync(string name, int nr, string securityLevel)
    {
        await using var dbConnection = new MySqlConnection(_connectionString);
        await dbConnection.OpenAsync();

        var cmd = new MySqlCommand(@"
        INSERT INTO Incident (Namn, NR, Säkerhetsgrad)
        VALUES (@namn, @nr, @sakerhetsgrad);", dbConnection);

        cmd.Parameters.AddWithValue("@namn", name);
        cmd.Parameters.AddWithValue("@nr", nr);
        cmd.Parameters.AddWithValue("@sakerhetsgrad", securityLevel);

        await cmd.ExecuteNonQueryAsync();
    }
}

