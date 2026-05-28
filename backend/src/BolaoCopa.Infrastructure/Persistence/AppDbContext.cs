using BolaoCopa.Domain.Entities;
using BolaoCopa.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BolaoCopa.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    private static readonly ValueConverter<DateTime, DateTime> UtcDateTimeConverter = new(
        value => value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value, DateTimeKind.Utc),
        value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

    public DbSet<User> Users => Set<User>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<Bet> Bets => Set<Bet>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        configureUser(modelBuilder);
        configureTeam(modelBuilder);
        configureMatch(modelBuilder);
        configureBet(modelBuilder);
    }

    private static void configureUser(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<User>();

        entity.ToTable("Users");
        entity.HasKey(user => user.Id);

        entity.Property(user => user.Name)
            .HasMaxLength(120)
            .IsRequired();

        entity.Property(user => user.Email)
            .HasMaxLength(320)
            .IsRequired();

        entity.HasIndex(user => user.Email)
            .IsUnique();

        entity.Property(user => user.PasswordHash)
            .HasMaxLength(512)
            .IsRequired();

        entity.Property(user => user.ShowBetsPublicly)
            .HasDefaultValue(false)
            .IsRequired();

        configureUtcDate(entity.Property(user => user.CreatedAt));
    }

    private static void configureTeam(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Team>();

        entity.ToTable("Teams");
        entity.HasKey(team => team.Id);

        entity.Property(team => team.Name)
            .HasMaxLength(120)
            .IsRequired();

        entity.Property(team => team.Code)
            .HasMaxLength(10)
            .IsRequired();

        entity.HasIndex(team => team.Code)
            .IsUnique();

        entity.Property(team => team.FlagUrl)
            .HasMaxLength(2048)
            .IsRequired();
    }

    private static void configureMatch(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Match>();

        entity.ToTable("Matches");
        entity.HasKey(match => match.Id);

        entity.HasOne(match => match.HomeTeam)
            .WithMany(team => team.HomeMatches)
            .HasForeignKey(match => match.HomeTeamId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        entity.HasOne(match => match.AwayTeam)
            .WithMany(team => team.AwayMatches)
            .HasForeignKey(match => match.AwayTeamId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        entity.Property(match => match.HomeGoals)
            .IsRequired(false);

        entity.Property(match => match.AwayGoals)
            .IsRequired(false);

        configureUtcDate(entity.Property(match => match.MatchDate));
        configureUtcDate(entity.Property(match => match.AllowBetUntil));

        entity.Property(match => match.IsBettingLocked)
            .HasDefaultValue(false)
            .IsRequired();

        entity.Property(match => match.Stage)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        entity.Property(match => match.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();
    }

    private static void configureBet(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Bet>();

        entity.ToTable("Bets");
        entity.HasKey(bet => bet.Id);

        entity.HasOne(bet => bet.User)
            .WithMany(user => user.Bets)
            .HasForeignKey(bet => bet.UserId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        entity.HasOne(bet => bet.Match)
            .WithMany(match => match.Bets)
            .HasForeignKey(bet => bet.MatchId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        entity.HasIndex(bet => new { bet.UserId, bet.MatchId })
            .IsUnique();

        entity.Property(bet => bet.HomeGoalsPrediction)
            .IsRequired();

        entity.Property(bet => bet.AwayGoalsPrediction)
            .IsRequired();

        entity.Property(bet => bet.PointsEarned)
            .IsRequired();

        configureUtcDate(entity.Property(bet => bet.CreatedAt));
    }

    private static void configureUtcDate(PropertyBuilder<DateTime> propertyBuilder)
    {
        propertyBuilder
            .HasColumnType("datetime2")
            .HasConversion(UtcDateTimeConverter);
    }
}
