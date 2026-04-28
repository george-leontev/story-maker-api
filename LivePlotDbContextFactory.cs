using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using StoryMakerApi.Data;
using DotNetEnv;

namespace StoryMakerApi.Data
{
    public class LivePlotDbContextFactory : IDesignTimeDbContextFactory<LivePlotDbContext>
    {
        public LivePlotDbContext CreateDbContext(string[] args)
        {
            // Загружаем переменные окружения, так как CLI их не подхватывает автоматически
            Env.Load();

            var dbServer = Environment.GetEnvironmentVariable("DB_SERVER");
            var dbPort = Environment.GetEnvironmentVariable("DB_PORT");
            var dbName = Environment.GetEnvironmentVariable("DB_NAME");

            // Настраиваем строку подключения (как мы делали ранее)
            var connBuilder = new SqlConnectionStringBuilder
            {
                DataSource = string.IsNullOrEmpty(dbPort) ? dbServer : $"{dbServer},{dbPort}",
                InitialCatalog = dbName,
                IntegratedSecurity = true, // Используем Windows Authentication
                TrustServerCertificate = true,
                MultipleActiveResultSets = true
            };

            var optionsBuilder = new DbContextOptionsBuilder<LivePlotDbContext>();
            optionsBuilder.UseSqlServer(connBuilder.ConnectionString);

            return new LivePlotDbContext(optionsBuilder.Options);
        }
    }
}