using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;
using MongoVacancyReview = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.VacancyReview;
using MongoManualQaOutcome = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.ManualQaOutcome;
using MongoRuleSetDecision = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.RuleSetDecision;
using MongoRuleId = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.RuleId;
using MongoRuleOutcome = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.RuleOutcome;

namespace SFA.DAS.Recruit.Jobs.Features.VacancyReviewMigration;

[ExcludeFromCodeCoverage]
public class VacancyReviewMapper(ILogger<VacancyReviewMapper> logger)
{
    public VacancyReview MapFrom(MongoVacancyReview source)
    {
        return new VacancyReview
        {
            Id = source.Id,
            VacancyReference = source.VacancyReference,
            VacancyTitle = source.VacancySnapshot.Title,
            CreatedDate = source.CreatedDate!.Value,
            SlaDeadline = source.SlaDeadline!.Value,
            ReviewedDate = source.ReviewedDate,
            Status = MapStatus(source),
            SubmissionCount = source.SubmissionCount,
            ReviewedByUserEmail = source.ReviewedByUser?.Email,
            SubmittedByUserEmail = !string.IsNullOrEmpty(source.SubmittedByUser.Email) ? source.SubmittedByUser.Email : "unknown",
            ClosedDate = source.ClosedDate,
            ManualOutcome = MapQaOutcome(source),
            ManualQaComment = source.ManualQaComment,
            ManualQaFieldIndicators = MapManualQaFieldIndicators(source),
            AutomatedQaOutcome = MapRuleSetOutcome(source),
            AutomatedQaOutcomeIndicators = MapAutomatedQaOutcomeIndicators(source),
            DismissedAutomatedQaOutcomeIndicators = source.DismissedAutomatedQaOutcomeIndicators ?? [],
            UpdatedFieldIdentifiers = source.UpdatedFieldIdentifiers,
            VacancySnapshot = JsonSerializer.Serialize(source.VacancySnapshot),
        };
    }

    private static string? MapAutomatedQaOutcomeIndicators(MongoVacancyReview source)
    {
        return source.AutomatedQaOutcomeIndicators is { Count: > 0 }
            ? $"{source.AutomatedQaOutcomeIndicators.First().IsReferred}"
            : null;
    }

    private static RuleSetOutcome? MapRuleSetOutcome(MongoVacancyReview source)
    {
        return source.AutomatedQaOutcome is null
            ? null
            : new RuleSetOutcome
            {
                RuleOutcomes = source.AutomatedQaOutcome.RuleOutcomes?.Select(MapRuleOutcome).ToList() ?? [],
                Decision = source.AutomatedQaOutcome.Decision switch
                {
                    MongoRuleSetDecision.Unknown => RuleSetDecision.Unknown,
                    MongoRuleSetDecision.Refer => RuleSetDecision.Refer,
                    MongoRuleSetDecision.Approve => RuleSetDecision.Approve,
                    MongoRuleSetDecision.Indeterminate => RuleSetDecision.Indeterminate,
                    _ => throw new ArgumentOutOfRangeException()
                }
            };
    }

    private static RuleOutcome MapRuleOutcome(MongoRuleOutcome source)
    {
        return new RuleOutcome
        {
            Id = source.Id,
            Details = source.Details?.Select(MapRuleOutcome),
            RuleId = source.RuleId switch
            {
                MongoRuleId.ProfanityChecks => RuleId.ProfanityChecks,
                MongoRuleId.BannedPhraseChecks => RuleId.BannedPhraseChecks,
                MongoRuleId.TitlePopularity => RuleId.TitlePopularity,
                MongoRuleId.VacancyAnonymous => RuleId.VacancyAnonymous,
                _ => throw new ArgumentOutOfRangeException()
            },
            Score = source.Score,
            Narrative = source.Narrative,
            Data = source.Data,
            Target = source.Target,
        };
    }

    private static ReviewStatus MapStatus(MongoVacancyReview source)
    {
        return source.Status switch {
            DataAccess.MongoDb.Domain.ReviewStatus.New => ReviewStatus.New,
            DataAccess.MongoDb.Domain.ReviewStatus.PendingReview => ReviewStatus.PendingReview,
            DataAccess.MongoDb.Domain.ReviewStatus.UnderReview => ReviewStatus.UnderReview,
            DataAccess.MongoDb.Domain.ReviewStatus.Closed => ReviewStatus.Closed,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private static List<ManualQaFieldIndicator> MapManualQaFieldIndicators(MongoVacancyReview source)
    {
        return source.ManualQaFieldIndicators?
            .Select(x => new ManualQaFieldIndicator
            {
                FieldIdentifier = x.FieldIdentifier,
                IsChangeRequested = x.IsChangeRequested,
            })
            .Where(x => x.IsChangeRequested)
            .ToList() ?? [];
    }

    private static ManualQaOutcome? MapQaOutcome(MongoVacancyReview source)
    {
        return source.ManualOutcome switch {
            MongoManualQaOutcome.Approved => ManualQaOutcome.Approved,
            MongoManualQaOutcome.Referred => ManualQaOutcome.Referred,
            MongoManualQaOutcome.Transferred => ManualQaOutcome.Transferred,
            MongoManualQaOutcome.Blocked => ManualQaOutcome.Blocked,
            null => null,
            _ => throw new ArgumentOutOfRangeException(nameof(source), source, null)
        };
    }
}