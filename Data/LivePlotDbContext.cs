using Microsoft.EntityFrameworkCore;
using StoryMakerApi.Models;
namespace StoryMakerApi.Data;

public sealed class LivePlotDbContext : DbContext
{
    public LivePlotDbContext(DbContextOptions<LivePlotDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Story> Stories => Set<Story>();
    public DbSet<Chapter> Chapters => Set<Chapter>();
    public DbSet<Choice> Choices => Set<Choice>();
    public DbSet<Vote> Votes => Set<Vote>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<StoryRating> StoryRatings => Set<StoryRating>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Vote>()
            .HasKey(v => new { v.UserId, v.ChoiceId });

        modelBuilder.Entity<Subscription>()
            .HasKey(s => new { s.UserId, s.StoryId });

        modelBuilder.Entity<Chapter>()
            .HasOne(c => c.Choice)
            .WithOne(ch => ch.Chapter)
            .HasForeignKey<Choice>(ch => ch.ChapterId);

        modelBuilder.Entity<Story>()
            .HasOne(s => s.Author)
            .WithMany(u => u.Stories)
            .HasForeignKey(s => s.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Story)
            .WithMany(s => s.Comments)
            .HasForeignKey(c => c.StoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Comment>()
            .HasOne(c => c.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Vote>()
            .HasOne(v => v.User)
            .WithMany(u => u.Votes)
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Vote>()
            .HasOne(v => v.Choice)
            .WithMany(c => c.Votes)
            .HasForeignKey(v => v.ChoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Subscription>()
            .HasOne(s => s.User)
            .WithMany(u => u.Subscriptions)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Subscription>()
            .HasOne(s => s.Story)
            .WithMany(st => st.Subscriptions)
            .HasForeignKey(s => s.StoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StoryRating>()
            .HasOne(r => r.User)
            .WithMany(u => u.Ratings)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<StoryRating>()
            .HasIndex(r => new { r.UserId, r.StoryId })
            .IsUnique();
    }
}
