using System.Data.SQLite;

namespace SeleniumBaseApi.Services
{
    public static class SQLiteManagerService
    {
        private static SQLiteConnection sqliteConnection;
        static SQLiteManagerService()
        {
            string baseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..");
            string workingDirectory = Path.GetFullPath(baseDirectory);
            string dataBasePath = Path.Combine(workingDirectory, "WebPagesLog.sqlite");
            if (!File.Exists(dataBasePath))
            {
                SQLiteConnection.CreateFile(dataBasePath);
                using (var temporaryConnection = new SQLiteConnection("Data Source=" + dataBasePath))
                {
                    temporaryConnection.Open();
                    string commandQuery = "create table WebPages (URL varchar(2048))";
                    SQLiteCommand command = new SQLiteCommand(commandQuery, temporaryConnection);
                    command.ExecuteNonQuery();
                }
            }
            sqliteConnection = new SQLiteConnection("Data Source=" + dataBasePath);
            sqliteConnection.Open();
        }

        public static async Task<bool> InsertWebPages(string query)
        {
            SQLiteCommand command = new SQLiteCommand(query, sqliteConnection);
            try
            {
                await command.ExecuteNonQueryAsync();
                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public static async Task<Queue<string>> CreateWebsitesQueue()
        {
            string commandQuery = "SELECT * FROM WebPages LIMIT 2000;";
            SQLiteCommand command = new SQLiteCommand(commandQuery, sqliteConnection);
            Queue<string> queue = new Queue<string>();
            using (var reader = await command.ExecuteReaderAsync())
            {
                if (reader.HasRows)
                {
                    while(await reader.ReadAsync())
                    {
                        queue.Enqueue(reader.GetString(0));
                    }
                }
            }
            string deleteQuery = "DELETE FROM WebPages LIMIT 2000;";
            SQLiteCommand deleteCommand = new SQLiteCommand(deleteQuery, sqliteConnection);
            await deleteCommand.ExecuteNonQueryAsync();
            return queue;
        }
    }
}
