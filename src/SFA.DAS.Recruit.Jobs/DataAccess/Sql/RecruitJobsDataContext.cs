using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using SFA.DAS.Recruit.Jobs.Domain;
using AvailableWhere = SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain.AvailableWhere;
using ClosureReason = SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain.ClosureReason;
using EmployerNameOption = SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain.EmployerNameOption;
using Vacancy = SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain.Vacancy;
using VacancyStatus = SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain.VacancyStatus;

namespace SFA.DAS.Recruit.Jobs.DataAccess.Sql;

[ExcludeFromCodeCoverage]
public class RecruitJobsDataContext(IOptions<SqlServerConfiguration> config, DbContextOptions options) : DbContext(options)
{
    private readonly SqlServerConfiguration _configuration = config.Value;
    
    public DbSet<ApplicationReview> ApplicationReview { get; set; }
    public DbSet<LegacyApplication> LegacyApplication { get; set; }
    public DbSet<EmployerProfile> EmployerProfile { get; set; }
    public DbSet<EmployerProfileAddress> EmployerProfileAddress { get; set; }
    public DbSet<VacancyReview> VacancyReview { get; set; }
    public DbSet<Vacancy> Vacancy { get; set; }
    public DbSet<User> User { get; set; }
    public DbSet<UserEmployerAccount> UserEmployerAccount { get; set; }
    public DbSet<Report> Report { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connection = new SqlConnection
        {
            ConnectionString = _configuration!.ConnectionString,
        };

        optionsBuilder.UseSqlServer(connection, options => options.EnableRetryOnFailure(5, TimeSpan.FromSeconds(20), null));
    }

    public static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ApplicationReview
        modelBuilder.Entity<ApplicationReview>().Property(e => e.Status).HasConversion(v => v.ToString(), v => (ApplicationReviewStatus)Enum.Parse(typeof(ApplicationReviewStatus), v));

        // EmployerProfile
        modelBuilder.Entity<EmployerProfile>().Property(x => x.AccountLegalEntityId).HasColumnType("bigint").ValueGeneratedNever();

        // VacancyReview
        modelBuilder.Entity<VacancyReview>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Status).HasConversion(v => v.ToString(), v => Enum.Parse<ReviewStatus>(v)).HasColumnType("nvarchar(100)");
            entity.Property(x => x.ManualOutcome).HasConversion(v => v.HasValue ? v.Value.ToString() : null, v => string.IsNullOrWhiteSpace(v) ? null : Enum.Parse<ManualQaOutcome>(v)).HasColumnType("nvarchar(50)");
            entity.Property(x => x.OwnerType).HasConversion<byte>();
            entity.Property(x => x.SubmissionCount).HasConversion<byte>();
            entity.Property(x => x.UpdatedFieldIdentifiers).HasConversion(x => JsonSerializer.Serialize(x, JsonOptions), x => JsonSerializer.Deserialize<List<string>>(x, JsonOptions));
            entity.Property(x => x.DismissedAutomatedQaOutcomeIndicators).HasConversion(x => JsonSerializer.Serialize(x, JsonOptions), x => JsonSerializer.Deserialize<List<string>>(x, JsonOptions));
            entity.Property(x => x.ManualQaFieldIndicators).HasConversion(x => JsonSerializer.Serialize(x, JsonOptions), x => JsonSerializer.Deserialize<List<string>>(x, JsonOptions));
            entity.Property(x => x.AutomatedQaOutcome).HasConversion(v => v != null ? v.ToString() : null, v => null);
        });

        // Vacancy
        modelBuilder.Entity<Vacancy>().Property(x => x.Status).HasConversion(v => v.ToString(), v => Enum.Parse<VacancyStatus>(v));
        modelBuilder.Entity<Vacancy>().Property(x => x.OwnerType).HasConversion(v => v.ToString(), v => Enum.Parse<OwnerType>(v!));
        modelBuilder.Entity<Vacancy>().Property(x => x.SourceOrigin).HasConversion(v => v.ToString(), v => Enum.Parse<SourceOrigin>(v!));
        modelBuilder.Entity<Vacancy>().Property(x => x.SourceType).HasConversion(v => v.ToString(), v => Enum.Parse<SourceType>(v!));
        modelBuilder.Entity<Vacancy>().Property(x => x.ArchiveType).HasConversion(v => v.ToString(), v => Enum.Parse<ArchiveType>(v!));
        modelBuilder.Entity<Vacancy>().Property(x => x.ApplicationMethod).HasConversion(v => v.ToString(), v => Enum.Parse<ApplicationMethod>(v!));
        modelBuilder.Entity<Vacancy>().Property(x => x.EmployerNameOption).HasConversion(v => v.ToString(), v => Enum.Parse<EmployerNameOption>(v!));
        modelBuilder.Entity<Vacancy>().Property(x => x.GeoCodeMethod).HasConversion(v => v.ToString(), v => Enum.Parse<GeoCodeMethod>(v!));
        modelBuilder.Entity<Vacancy>().Property(x => x.Wage_DurationUnit).HasConversion(v => v.ToString(), v => Enum.Parse<DurationUnit>(v!));
        modelBuilder.Entity<Vacancy>().Property(x => x.Wage_WageType).HasConversion(v => v.ToString(), v => Enum.Parse<WageType>(v!));
        modelBuilder.Entity<Vacancy>().Property(x => x.ClosureReason).HasConversion(v => v.ToString(), v => Enum.Parse<ClosureReason>(v!));
        modelBuilder.Entity<Vacancy>().Property(x => x.ApprenticeshipType).HasConversion(v => v.ToString(), v => Enum.Parse<ApprenticeshipTypes>(v!));
        modelBuilder.Entity<Vacancy>().Property(x => x.Wage_FixedWageYearlyAmount).HasColumnType("decimal");
        modelBuilder.Entity<Vacancy>().Property(x => x.Wage_WeeklyHours).HasColumnType("decimal");
        modelBuilder.Entity<Vacancy>().Property(x => x.EmployerLocationOption).HasConversion(v => v.ToString(), v => Enum.Parse<AvailableWhere>(v!));
        modelBuilder.Entity<Vacancy>().Property(v => v.Qualifications)
            .HasConversion(x => JsonSerializer.Serialize(x, JsonOptions), x => JsonSerializer.Deserialize<List<Qualification>>(x, JsonOptions));
        modelBuilder.Entity<Vacancy>().Property(v => v.EmployerLocations)
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<List<Address>>(v, JsonOptions));
        // User
        var userBuilder = modelBuilder.Entity<User>();
        userBuilder.ToTable("User").HasMany(x => x.EmployerAccounts).WithOne(x => x.User).HasForeignKey(x => x.UserId);
        userBuilder.HasKey(x => x.Id);
        userBuilder.Property(x => x.UserType).HasConversion(v => v.ToString(), v => Enum.Parse<UserType>(v!));
        
        // UserEmployerAccount
        var userEmployerAccountBuilder = modelBuilder.Entity<UserEmployerAccount>();
        userEmployerAccountBuilder.ToTable("UserEmployerAccount").HasOne(x => x.User).WithMany(x => x.EmployerAccounts).HasForeignKey(x => x.UserId);
        userEmployerAccountBuilder.HasKey(x => new { x.UserId, x.EmployerAccountId });

        // Report
        modelBuilder.Entity<Report>().HasKey(x => x.Id);
        modelBuilder.Entity<Report>().Property(e => e.Type).HasConversion(v => v.ToString(), v => Enum.Parse<ReportType>(v));
        modelBuilder.Entity<Report>().Property(e => e.OwnerType).HasConversion(v => v.ToString(), v => Enum.Parse<ReportOwnerType>(v));

        // BlockedOrganisation
        modelBuilder.Entity<BlockedOrganisation>().ToTable("BlockedOrganisation");
        modelBuilder.Entity<BlockedOrganisation>().HasKey("Id");
        modelBuilder.Entity<BlockedOrganisation>().Property(x => x.BlockedStatus).HasConversion(v => v.ToString(), v => Enum.Parse<BlockedStatus>(v));
        modelBuilder.Entity<BlockedOrganisation>().Property(x => x.OrganisationType).HasConversion(v => v.ToString(), v => Enum.Parse<OrganisationType>(v));
    }
}