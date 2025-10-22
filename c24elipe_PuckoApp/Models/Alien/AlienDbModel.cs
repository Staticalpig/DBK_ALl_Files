using System.Data;
using System.Data.Common;
using Microsoft.AspNetCore.Mvc.Rendering;
using MySql.Data.MySqlClient;

namespace c24elipe_PuckoApp.Models;

public class AlienDbModel
{
    private IConfiguration _config;
    private readonly string _connectionString;

    public AlienDbModel(IConfiguration config, string userRole)
    {
        _config = config;
        
         userRole = userRole ?? "admin";
        _connectionString = _config[userRole];
        
    }
    public async Task<List<AlienModel>> GetAllAliensAsync()
        {
            List<AlienModel> aliens = new List<AlienModel>();
            await using (MySqlConnection dbConnection = new MySqlConnection(_connectionString))
            {
                await dbConnection.OpenAsync();
                MySqlCommand cmd = new MySqlCommand("SELECT * FROM c24elipe.wv_Alien_Ras_Full", dbConnection);

                await using (DbDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        AlienModel alien = new AlienModel
                        {
                            PNR = reader.GetString("PNR"),
                            Farlighet = reader.GetString("Farlighet"),
                            Alias = reader.IsDBNull("Alias") ? "" : reader.GetString("Alias"),
                            Hemplanet = reader.IsDBNull("Hemplanet") ? "" : reader.GetString("Hemplanet"),
                            RasNamn = reader.GetString("RasNamn"),
                            Beskrivning = reader.IsDBNull("Beskrivning") ? "" : reader.GetString("Beskrivning"),
                            ArRegistrerad = reader.GetBoolean("ÄrRegistrerad")
                        };
                        aliens.Add(alien);
                    }
                }
            }
            return aliens;
        }
        
        // NEW: Gets all alien races for the dropdown
        public async Task<List<SelectListItem>> GetAlienRacesAsync()
        {
            var races = new List<SelectListItem>();
            await using (MySqlConnection dbConnection = new MySqlConnection(_connectionString))
            {
                await dbConnection.OpenAsync();
                MySqlCommand cmd = new MySqlCommand("SELECT RasNamn FROM c24elipe.Ras;", dbConnection);

                await using (DbDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        string raceName = reader.GetString("RasNamn");
                        races.Add(new SelectListItem { Text = raceName, Value = raceName });
                    }
                }
            }
            return races;
        }

        // NEW: Calls the stored procedure to create an alien
        public async Task CreateAlienAsync(CreateAlienViewModel alien)
        {
            await using (MySqlConnection dbConnection = new MySqlConnection(_connectionString))
            {
                await dbConnection.OpenAsync();
          
                MySqlCommand cmd = new MySqlCommand("c24elipe.sp_app_create_alien", dbConnection);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@p_PNR", alien.PNR);
                cmd.Parameters.AddWithValue("@p_Ras", alien.Ras);
                cmd.Parameters.AddWithValue("@p_Hemplanet", string.IsNullOrEmpty(alien.Hemplanet) ? DBNull.Value : alien.Hemplanet);
                cmd.Parameters.AddWithValue("@p_KändaNamn", string.IsNullOrEmpty(alien.KändaNamn) ? DBNull.Value : alien.KändaNamn);
                cmd.Parameters.AddWithValue("@p_ÄrRegistrerad", alien.ÄrRegistrerad);
                cmd.Parameters.AddWithValue("@p_Farlighet", alien.Farlighet);

                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
