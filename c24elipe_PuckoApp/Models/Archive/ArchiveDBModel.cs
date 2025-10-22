using MySql.Data.MySqlClient;

namespace c24elipe_PuckoApp.Models.Archive;

public class ArchiveDBModel
{
    private IConfiguration _config;
    private readonly string _connectionString;

    public ArchiveDBModel(IConfiguration config, string userRole)
    {
        _config = config;

        userRole = userRole ?? "admin";
        _connectionString = _config[userRole];
    }
    
    
    public async Task ArchiveAlienAsync()
    {
       
            await using var dbConnection = new MySqlConnection(_connectionString);
            await dbConnection.OpenAsync();
        
            var cmd = new MySqlCommand("CALL c24elipe.sp_archive_old_rapport();", dbConnection);
        
            await cmd.ExecuteNonQueryAsync();
    }
    
    public async Task ArchiveRapportAsync()
    {
       
            const string quary = @"DELETE A
                                    FROM Alien AS A
                                    WHERE NOT EXISTS(
                                        SELECT 1
                                    FROM Incident_Alien AS IA
                                    WHERE IA.Alien_IdKod = A.IdKod
                                        )";
        
            await using var dbConnection = new MySqlConnection(_connectionString);
            await dbConnection.OpenAsync();
            
            var cmd = new MySqlCommand(quary, dbConnection);
        
            await cmd.ExecuteNonQueryAsync();
    }
    
}