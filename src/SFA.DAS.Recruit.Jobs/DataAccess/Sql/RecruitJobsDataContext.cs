using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

namespace SFA.DAS.Recruit.Jobs.DataAccess.Sql;

public class RecruitJobsDataContext(IOptions<RecruitJobsConfiguration> config, DbContextOptions options) : DbContext(options)
{
    private readonly RecruitJobsConfiguration _configuration = config.Value;
    
    public DbSet<ApplicationReview> ApplicationReview { get; set; }
    
    public DbSet<LegacyApplication> LegacyApplication { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connection = new SqlConnection
        {
            ConnectionString = _configuration!.ConnectionString,
        };

        optionsBuilder.UseSqlServer(connection, options => options.EnableRetryOnFailure(5, TimeSpan.FromSeconds(20), null));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<ApplicationReview>()
            .Property(e => e.Status)
            .HasConversion(
                v => v.ToString(),
                v => (ApplicationReviewStatus)Enum.Parse(typeof(ApplicationReviewStatus), v));
    }
}