using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

public class UserEmployerAccount
{
    [Key]
    [Column(Order = 1)]
    public Guid UserId { get; set; }
    
    [Key]
    [Column(Order = 2)]
    public long EmployerAccountId { get; set; }
    
    public virtual User User { get; set; }
}