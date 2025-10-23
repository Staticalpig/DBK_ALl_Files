using System.Data;
using System.Data.Common;
using c24elipe_PuckoApp.Models.Incident;
using c24elipe_PuckoApp.Models.Raport;
using MySql.Data.MySqlClient;

namespace c24elipe_PuckoApp.Repositories;

public class RapportRepository
{
    private readonly IConfiguration _config;
    private readonly ILogger<RapportRepository> _logger;
    private readonly string userRole;
    private readonly string connectionString;
    

    public RapportRepository(IConfiguration config, string userRole)
    {
        _config = config;
        _logger = LoggerFactory.Create(builder => 
            builder.AddConsole()).CreateLogger<RapportRepository>();
            
        userRole = userRole ?? "agent";
        this.userRole = userRole;
        connectionString = _config[userRole];
    }
    
    public async Task<List<RapportDBModel>> GetAllRapports(string username)
    { 
        _logger.LogInformation("Fetching all incidents for user role: {UserRole}, the connection string is  {ConnectionString}", 
            userRole, connectionString);
        
        string quary = string.Empty;

        if (userRole == "agent")
        {
            quary = @"SELECT * FROM vw_get_full_rapport WHERE Användarnamn = @Användarnamn;";
        }
        else
        {
            quary = @"SELECT * FROM vw_get_full_rapport;";
        }
        
        await using var dbConnection = new MySqlConnection(connectionString);
        await dbConnection.OpenAsync();
        var cmd = new MySqlCommand(quary, dbConnection);

        if (userRole == "agent")
        {
            cmd.Parameters.AddWithValue("@Användarnamn", username);
        }
        
        var incidents = new List<RapportDBModel>();
        await using DbDataReader reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var rapport = new RapportDBModel
            {
                Datum = reader.GetDateTime("Datum"),
                Nr = reader.GetInt32("Nr"),
                RapportTyp = reader.GetString("RapportTyp"),
                Agent_ID = reader.GetInt32("Agent_ID"),
                Användarnamn = reader.GetString("Användarnamn"),
                Incidentledare = reader.GetString("Incidentledare"),
                Slutdatum = reader.GetDateTime("Slutdatum"),
                Incident_Namn =  reader.GetString("Incident_Namn"),
                Incident_NR = reader.GetInt32("Incident_NR")
            };
            incidents.Add(rapport);
        }
        
        return incidents;
    }

    public async Task<List<RapportDBRadderModel>> GetRappportRadder(DateTime rapportDatum, string rapportNr)
    {
        _logger.LogInformation(
            "Fetching rapport with Nr: {RapportNr} for user role: {UserRole}, the connection string is  {ConnectionString}",
            rapportNr, userRole, connectionString);

        const string quary = @"SELECT rr.nr, rr.Text
                         FROM Rapport_Rader as rr
                         WHERE Rapport_Datum = @Rapport_Datum
                            AND Rapport_Nr = @Rapport_Nr
                         ORDER BY rr.nr;";

        await using var dbConnection = new MySqlConnection(connectionString);

        await dbConnection.OpenAsync();

        var cmd = new MySqlCommand(quary, dbConnection);
        cmd.Parameters.AddWithValue("@Rapport_Datum", rapportDatum);
        cmd.Parameters.AddWithValue("@Rapport_Nr", rapportNr);

        var rapportRader = new List<RapportDBRadderModel>();
        await using DbDataReader reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var rapportRad = new RapportDBRadderModel
            {
                Nr = reader.GetInt32("nr"),
                Text = reader.GetString("Text")
            };
            rapportRader.Add(rapportRad);
        }
        
        _logger.LogInformation("Fetched {Count} rapport radder from the database.", rapportRader.Count);
        
        return rapportRader;
    }

    public async Task<List<RapportDBCommentModel>> GetRapportComments(DateTime rapportDatum, string rapportNr)
    {
        _logger.LogInformation(
            "Fetching rapport comments with Nr: {RapportNr} for user role: {UserRole}, the connection string is  {ConnectionString}",
            rapportNr, userRole, connectionString);
        
        const string quary = @"SELECT * FROM c24elipe.wv_Rapport_Kommentar_Full 
                                WHERE Rapport_Datum = @rapportDatum 
                                  AND Rapport_Nr = @rapportNr ;";
        
        await using var dbConnection = new MySqlConnection(connectionString);
        await dbConnection.OpenAsync();
        var cmd = new MySqlCommand(quary, dbConnection);
        cmd.Parameters.AddWithValue("@rapportDatum", rapportDatum);
        cmd.Parameters.AddWithValue("@rapportNr", rapportNr);
        var rapportComments = new List<RapportDBCommentModel>();
        
        await using DbDataReader reader = await cmd.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            var rapportComment = new RapportDBCommentModel
            {
                Rapport_Datum = reader.GetDateTime("Rapport_Datum"),
                Rapport_Nr = reader.GetInt32("Rapport_Nr"),
                Nr = reader.GetInt32("Nr"),
                Text = reader.GetString("Text"),
                GjordAvNamn = reader.IsDBNull(reader.GetOrdinal("GjordAv"))
                    ? null
                    : reader.GetString("GjordAv"),
            };
            rapportComments.Add(rapportComment);
            
        }
        
        _logger.LogInformation("Fetched {Count} rapport comments from the database.", rapportComments.Count);
            
        return rapportComments;
    }

    public async Task CreateNewRad(string rapportDatum, int rapportNr, string text, string userRole)
    {
        if (userRole == "agent")
        {
            _logger.LogWarning("!!! Unauthorized attempt to create new rapport rad by user role: {UserRole}", userRole);
            return;
        }
        
        _logger .LogInformation(
            "->>>>> Creating new rapport rad for Rapport Nr: {RapportNr} by user role: {UserRole}, the connection string is  {ConnectionString}",
            rapportNr, userRole, connectionString);
        
        const string query = @"
        INSERT INTO Rapport_Rader (Rapport_Datum, Rapport_Nr, nr, Text)
        SELECT @rapportDatum, 
               @rapportNr,
               COALESCE(MAX(nr), 0) + 1,
               @text
        FROM Rapport_Rader
        WHERE Rapport_Datum = @rapportDatum AND Rapport_Nr = 24;";

        await using var dbConnection = new MySqlConnection(connectionString);
        await dbConnection.OpenAsync();

        var cmd = new MySqlCommand(query, dbConnection);
    
 
        cmd.Parameters.AddWithValue("@rapportDatum", rapportDatum);
        cmd.Parameters.AddWithValue("@rapportNr", rapportNr);
        cmd.Parameters.AddWithValue("@text", text);

        var rowsAffected = await cmd.ExecuteNonQueryAsync();

        if (rowsAffected < 1)
        {
            _logger.LogError("!!! Failed to create new rapport rad for Rapport Nr: {RapportNr}. No rows were inserted.", rapportNr);
            throw new Exception("Failed to create new rapport rad. The operation affected 0 rows.");
        }
    
        _logger.LogInformation("New rapport rad created successfully for Rapport Nr: {RapportNr}.", rapportNr);
    
    }
}