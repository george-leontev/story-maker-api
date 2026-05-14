using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using StoryMakerApi.Data;
using StoryMakerApi.Helpers;
using DotNetEnv;

namespace StoryMakerApi.Data;

public class LivePlotDbContextFactory : IDesignTimeDbContextFactory<LivePlotDbContext>
{
    public LivePlotDbContext CreateDbContext(string[] args)
    {
        Env.Load();

        var optionsBuilder = new DbContextOptionsBuilder<LivePlotDbContext>();
        optionsBuilder.UseSqlServer(DbConnectionStringBuilder.BuildFromEnvironment());

        return new LivePlotDbContext(optionsBuilder.Options);
    }
}