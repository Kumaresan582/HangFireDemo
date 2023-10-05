using Microsoft.Data.SqlClient;

namespace HangFireDemo.DbBackUp
{
    public class DataBaseBackUp : IDataBaseBackUp
    {
        public string TriggerBackup()
        {
            try
            {
                string serverName = "192.168.0.252";
                string databaseName = "kumaresan";
                string userName = "sa";
                string password = "local@vaf@123";
                string backupPath = @"C:\Program Files\Microsoft SQL Server\MSSQL15.VISUALAPP\MSSQL\Backup\";
                string connectionString = $"Server={serverName};Database={databaseName};User Id={userName};Password={password};Encrypt=False";

                if(!Directory.Exists(backupPath))
                    Directory.CreateDirectory(backupPath);

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string backupCommand = $"BACKUP DATABASE [{databaseName}] TO DISK = '{Path.Combine(backupPath, $"{databaseName}_backup.bak")}'";

                    using (SqlCommand cmd = new SqlCommand(backupCommand, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    connection.Close();
                }

                return "Backup completed successfully.";
            }
            catch (Exception ex)
            {
                return $"Backup failed: {ex.Message}";
            }
        }
    }
}