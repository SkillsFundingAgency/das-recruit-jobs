using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

public class VacancyReview
{
    public static readonly VacancyReview None = new();
    
    public Guid Id { get; init; }
    public long VacancyReference { get; init; }
    public string VacancyTitle { get; init; }
    public DateTime CreatedDate { get; init; }
    public DateTime SlaDeadline { get; init; }
    public DateTime? ReviewedDate { get; init; }
    public ReviewStatus Status { get; init; }
    public string? ReviewedByUserEmail { get; init; }
    public string SubmittedByUserEmail { get; init; }
    public int SubmissionCount { get; init; }
    public DateTime? ClosedDate { get; init; }
    public ManualQaOutcome? ManualOutcome { get; init; }
    public string? ManualQaComment { get; init; }
    public string VacancySnapshot { get; init; }
    public List<string>? UpdatedFieldIdentifiers { get; init; }
    public List<string>? ManualQaFieldIndicators { get; init; }
    public RuleSetOutcome? AutomatedQaOutcome {get; init; }
    [MaxLength(20)]
    public string? AutomatedQaOutcomeIndicators {get; init; }
    public List<string>? DismissedAutomatedQaOutcomeIndicators { get; init; } = [];
}

public enum ManualQaOutcome : byte
{
    Approved,
    Referred,
    Transferred,
    Blocked
}

public enum ReviewStatus : byte
{
    New,
    PendingReview,
    UnderReview,
    Closed
}

public class RuleOutcomeIndicator
{
    public Guid RuleOutcomeId { get; set; }

    public bool IsReferred { get; set; }
}

public class RuleSetOutcome
{
    public List<RuleOutcome>? RuleOutcomes { get; set; } = new ();
    public RuleSetDecision Decision { get; set; }
}

public class RuleOutcome
{
    public Guid Id { get; set; }
    public IEnumerable<RuleOutcome>? Details { get; set; } = [];
    public RuleId RuleId { get; set; }
    public int Score { get; set;  }
    public string? Narrative { get; set;  }
    public string? Data { get; set; }
    public string? Target { get; set;  }
}

public enum RuleId
{
    ProfanityChecks,
    BannedPhraseChecks,
    TitlePopularity,
    VacancyAnonymous
}

public enum RuleSetDecision
{
    Unknown = 0,
    Refer,
    Approve,
    Indeterminate
}