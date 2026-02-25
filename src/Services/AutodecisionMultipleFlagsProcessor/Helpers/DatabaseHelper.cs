namespace AutodecisionMultipleFlagsProcessor.Helpers
{
    public class DatabaseHelper
    {
        public static string GetConnectionString(string databaseName, IConfiguration? configuration)
        {
            string? host = Environment.GetEnvironmentVariable("DB_HOST");
            string? user = Environment.GetEnvironmentVariable("DB_USER");
            string? password = Environment.GetEnvironmentVariable("DB_PASSWORD");
            string? sslMode = Environment.GetEnvironmentVariable("DB_SSL_MODE") ?? "None";

            if (string.IsNullOrEmpty(user) && configuration != null)
                user = configuration[$"databases:default:user"];
            if (string.IsNullOrEmpty(host) ||
                string.IsNullOrEmpty(user))
                throw new Exception("Some database configurations were not found.");
            if (string.IsNullOrEmpty(password))
                return $"Server={host}; Database={databaseName}; Uid={user};SSL Mode={sslMode};";
            return $"Server={host}; Database={databaseName}; Uid={user};Pwd={password}; SSL Mode={sslMode};";
        }
    }
}
