namespace SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;

public class VacancyReview
{
    public Guid Id { get; set; }
    public long VacancyReference { get; set; }
    public string Title { get; set; }
    public ReviewStatus Status { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? ReviewedDate { get; set; }
    public VacancyUser? ReviewedByUser { get; set; }
    public DateTime? ClosedDate { get; set; }
    public ManualQaOutcome? ManualOutcome { get; set; }
    public string ManualQaComment { get; set; }
    public List<ManualQaFieldIndicator> ManualQaFieldIndicators { get; set; }
    public string PrivateReviewNotes { get; set; }
    public VacancyUser SubmittedByUser { get; set; }
    public int SubmissionCount { get; set; }
    public DateTime? SlaDeadline { get; set; }
    public Vacancy VacancySnapshot { get; set; }
    public List<string> UpdatedFieldIdentifiers { get; set; }
    public bool CanApprove => Status == ReviewStatus.UnderReview;
    public bool CanRefer => Status == ReviewStatus.UnderReview;
    public bool CanUnassign => Status == ReviewStatus.UnderReview && ReviewedByUser != null;
    public bool IsPending => Status == ReviewStatus.New || Status == ReviewStatus.PendingReview;
    public RuleSetOutcome? AutomatedQaOutcome { get; set; }
    public List<RuleOutcomeIndicator> AutomatedQaOutcomeIndicators { get; set; } = new ();
    public IEnumerable<ManualQaFieldEditIndicator> ManualQaFieldEditIndicators { get; set; } = new List<ManualQaFieldEditIndicator>();
    public List<string>? DismissedAutomatedQaOutcomeIndicators { get; set; }
    
    public DateTime? MigrationDate { get; set; }
    public bool? MigrationFailed { get; set; }
    public bool? MigrationIgnore { get; set; }
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

public class ManualQaFieldIndicator
{
    public string FieldIdentifier { get; set; }
    public bool IsChangeRequested { get; set; }
}

public class RuleSetOutcome
{
    public List<RuleOutcome>? RuleOutcomes { get; set; } = [];
    public RuleSetDecision Decision { get; set; }
}

public class RuleOutcomeIndicator
{
    public Guid RuleOutcomeId { get; set; }

    public bool IsReferred { get; set; }
}

public class RuleOutcome
{
    public const string NoSpecificTarget = "";

    public RuleOutcome(RuleId ruleId, int score, string narrative, string target = NoSpecificTarget, IEnumerable<RuleOutcome> details = null, string data = null)
    {
        Id = Guid.NewGuid();
        RuleId = ruleId;
        Score = Math.Min(score, 100);
        Narrative = narrative;
        Target = target;
        Data = data;

        if (details != null)
            Details = details;
    }

    public Guid Id { get; set; }
    public IEnumerable<RuleOutcome>? Details { get; set; }
    public RuleId RuleId { get; set; }
    public int Score { get; set;  }
    public string Narrative { get; set;  }
    public string Data { get; set; }
    public string Target { get; set;  }
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

public class ManualQaFieldEditIndicator
{
    public string FieldIdentifier { get; set; }
    public string BeforeEdit { get; set; }
    public string AfterEdit { get; set; }
}