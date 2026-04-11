using Microsoft.EntityFrameworkCore;
using StoryMakerApi.Data;

DotNetEnv.Env.Load();

var dbServer = Environment.GetEnvironmentVariable("DB_SERVER");
var dbPort = Environment.GetEnvironmentVariable("DB_PORT");
var dbName = Environment.GetEnvironmentVariable("DB_NAME");
var dbUser = Environment.GetEnvironmentVariable("DB_USER");
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD");

var connectionString = $"Server={dbServer},{dbPort};Database={dbName};User Id={dbUser};Password={dbPassword};TrustServerCertificate=True;MultipleActiveResultSets=true";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<LivePlotDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/", () => "LivePlot API is running.")
    .WithName("HealthCheck")
    .WithOpenApi();

app.Run();
