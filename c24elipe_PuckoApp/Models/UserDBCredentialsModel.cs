using System.Diagnostics;
using MySql.Data.MySqlClient;

namespace c24elipe_PuckoApp.Models;

public class UserDbCredentialsModel
{
    private IConfiguration _config;
    private readonly string _connectionString;

    public UserDbCredentialsModel(IConfiguration config)
    {
        _config = config;
        _connectionString = _config["Default"];
        
    }

    public string GetUserHashedPassword(string username)
    {
        
        MySqlConnection dbConnection =
            new MySqlConnection(_connectionString);
        dbConnection.Open();

        MySqlCommand cmd = new MySqlCommand("CALL sp_get_agent_login(@username)", dbConnection);
        cmd.Parameters.AddWithValue("@username", username);
        // Only one column is returned, "Lösenord"

        MySqlDataReader reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            
            string hashedPassword = reader.GetString("Lösenord");
            dbConnection.Close();
            return hashedPassword;
        }
        dbConnection.Close();
        return string.Empty;
    }
    
    public List<string> GetUserDetails(string username)
    {
        
        MySqlConnection dbConnection =
            new MySqlConnection(_connectionString);
        dbConnection.Open();

        MySqlCommand cmd = new MySqlCommand("CALL c24elipe.sp_give_info_to_login(@username)", dbConnection);
        cmd.Parameters.AddWithValue("@username", username);
        // Only one column is returned, "Lösenord"

        MySqlDataReader reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            
            string fornamn = reader.GetString("Fornamn");
            string role = reader.GetString("Avdelning");
            dbConnection.Close();
            return new List<string> { fornamn, role };
        }
        dbConnection.Close();
        return new List<string>();
    }
}