using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;
public class VacancyAnalytics
{
    public required long VacancyReference { get; set; }
    public DateTime UpdatedDate { get; set; }

    [Column(TypeName = "nvarchar(max)")]
    [MaxLength(4000)]
    public string Analytics { get; set; } = string.Empty;
}