namespace StoryMakerApi.Helpers;

public static class DbConnectionStringBuilder
{
    public static string BuildFromEnvironment()
    {
        var dbServer = Environment.GetEnvironmentVariable("DB_SERVER")
            ?? throw new InvalidOperationException("Переменная DB_SERVER не задана.");

        var dbPort = Environment.GetEnvironmentVariable("DB_PORT");
        var dbName = Environment.GetEnvironmentVariable("DB_NAME")
            ?? throw new InvalidOperationException("Переменная DB_NAME не задана.");

        var integratedAuth = Environment.GetEnvironmentVariable("INTEGRATED_AUTH")?.ToLower() == "true";

        var dataSource = string.IsNullOrEmpty(dbPort) ? dbServer : $"{dbServer},{dbPort}";

        if (integratedAuth)
        {
            return $"Server={dataSource};Database={dbName};" +
                   "Integrated Security=true;" +
                   "TrustServerCertificate=True;" +
                   "MultipleActiveResultSets=true;";
        }

        var dbUser = Environment.GetEnvironmentVariable("DB_USER");
        var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");

        if (string.IsNullOrWhiteSpace(dbUser) || string.IsNullOrWhiteSpace(dbPassword))
        {
            throw new InvalidOperationException(
                "Для SQL Server аутентификации необходимы переменные DB_USER и DB_PASSWORD в .env");
        }

        return $"Server={dataSource};Database={dbName};" +
               $"User Id={dbUser};Password={dbPassword};" +
               "TrustServerCertificate=True;" +
               "MultipleActiveResultSets=true;";
    }
}
