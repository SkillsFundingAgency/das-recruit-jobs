using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

namespace SFA.DAS.Recruit.Jobs.DataAccess.Sql;

[ExcludeFromCodeCoverage]
public class RecruitJobsDataContext(IOptions<RecruitJobsConfiguration> config, DbContextOptions options) : DbContext(options)
{
    private readonly RecruitJobsConfiguration _configuration = config.Value;
    
    public DbSet<ApplicationReview> ApplicationReview { get; set; }
    public DbSet<LegacyApplication> LegacyApplication { get; set; }
    public DbSet<ProhibitedContent> ProhibitedContent { get; set; }
    public DbSet<EmployerProfile> EmployerProfile { get; set; }
    public DbSet<EmployerProfileAddress> EmployerProfileAddress { get; set; }
    public DbSet<UserNotificationPreferences> UserNotificationPreferences { get; set; }
    public DbSet<VacancyReview> VacancyReview { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connection = new SqlConnection
        {
            ConnectionString = _configuration!.ConnectionString,
        };

        optionsBuilder.UseSqlServer(connection, options => options.EnableRetryOnFailure(5, TimeSpan.FromSeconds(20), null));
    }

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ApplicationReview
        modelBuilder
            .Entity<ApplicationReview>()
            .Property(e => e.Status)
            .HasConversion(
                v => v.ToString(),
                v => (ApplicationReviewStatus)Enum.Parse(typeof(ApplicationReviewStatus), v));

        // ProhibitedContent
        modelBuilder
            .Entity<ProhibitedContent>()
            .HasKey(x => new { x.ContentType, x.Content });
        modelBuilder
            .Entity<ProhibitedContent>()
            .Property(e => e.ContentType)
            .HasConversion<int>();

        // UserNotificationPreferences
        modelBuilder
            .Entity<UserNotificationPreferences>()
            .HasKey(x => x.UserId);
        modelBuilder
            .Entity<UserNotificationPreferences>()
            .Property(x => x.Types)
            .HasConversion(
                v => v.ToString(),
                v => (NotificationTypes)Enum.Parse(typeof(NotificationTypes), v));
        modelBuilder
            .Entity<UserNotificationPreferences>()
            .Property(x => x.Frequency)
            .HasConversion(
                v => v.ToString(),
                v => (NotificationFrequency)Enum.Parse(typeof(NotificationFrequency), v));
        modelBuilder
            .Entity<UserNotificationPreferences>()
            .Property(x => x.Scope)
            .HasConversion(
                v => v.ToString(),
                v => (NotificationScope)Enum.Parse(typeof(NotificationScope), v));
        
        // EmployerProfile
        modelBuilder
            .Entity<EmployerProfile>()
            .Property(x => x.AccountLegalEntityId)
            .HasColumnType("bigint")
            .ValueGeneratedNever();

        // VacancyReview
        modelBuilder
            .Entity<VacancyReview>()
            .HasKey(x => x.Id);

        modelBuilder
            .Entity<VacancyReview>()
            .Property(x => x.UpdatedFieldIdentifiers)
            .HasConversion(
                x => JsonSerializer.Serialize(x, JsonOptions),
                x => JsonSerializer.Deserialize<List<string>>(x, JsonOptions)
            );
        
        modelBuilder
            .Entity<VacancyReview>()
            .Property(x => x.DismissedAutomatedQaOutcomeIndicators)
            .HasConversion(
                x => JsonSerializer.Serialize(x, JsonOptions),
                x => JsonSerializer.Deserialize<List<string>>(x, JsonOptions)
            );

        modelBuilder
            .Entity<VacancyReview>()
            .Property(x => x.ManualQaFieldIndicators)
            .HasConversion(
                x => JsonSerializer.Serialize(x, JsonOptions),
                x => JsonSerializer.Deserialize<List<string>>(x, JsonOptions)
            );
        
        modelBuilder
            .Entity<VacancyReview>()
            .Property(x => x.AutomatedQaOutcome)
            .HasConversion(
                x => JsonSerializer.Serialize(x, JsonOptions),
                x => JsonSerializer.Deserialize<RuleSetOutcome>(x, JsonOptions)!
            );
        
        modelBuilder
            .Entity<VacancyReview>()
            .Property(e => e.ManualOutcome)
            .HasConversion(
                v => v.ToString(),
                v => (ManualQaOutcome)Enum.Parse(typeof(ManualQaOutcome), v!));
        
        modelBuilder
            .Entity<VacancyReview>()
            .Property(e => e.Status)
            .HasConversion(
                v => v.ToString(),
                v => (ReviewStatus)Enum.Parse(typeof(ReviewStatus), v));
    }
}