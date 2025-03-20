using System;

namespace SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;

public class ApplicationReview
{
    public long AccountId { get; init; }
    public Guid? ApplicationId { get; init; }
    public string? CandidateFeedback { get; init; }
    public Guid CandidateId { get; init; }
    public DateTime CreatedDate { get; init; }
    public DateTime? DateSharedWithEmployer { get; init; }
    public bool HasEverBeenEmployerInterviewing { get; init; }
    public Guid Id { get; init; }
    public Guid? LegacyApplicationId { get; init; }
    public Owner Owner { get; init; }
    public int Ukprn { get; init; }
    public DateTime? ReviewedDate { get; init; }
    public ApplicationReviewStatus Status { get; init; }
    public Owner? StatusUpdatedBy { get; init; }
    public Guid? StatusUpdatedByUserId { get; init; }
    public DateTime SubmittedDate { get; init; }
    public long VacancyReference  { get; init; }
    public DateTime? WithdrawnDate { get; init; }
}

/*
CREATE TABLE dbo.[ApplicationReview] (
    [Id]					        uniqueidentifier	NOT NULL,
    [Ukprn]                         int NOT NULL,
    [AccountId]                     bigint NOT NULL,
    [Owner]                         tinyint NOT NULL, -- 0 Provider 1 Employer
    [CandidateFeedback]             nvarchar(max) NULL,
    [CandidateId]                   GUID not NULL,
    [CreatedDate]                   DATETIME NOT NULL,
    [DateSharedWithEmployer]        DATETIME NULL,
    [HasEverBeenEmployerInterviewing] BIT NOT NULL DEFAULT 0,
    [WithdrawnDate]                  DATETIME NULL,
    [ReviewedDate]                   DATETIME NULL,   
    [SubmittedDate]                  DATETIME NOT NULL,   
    [Status]                        tinyint not null
    [StatusUpdatedByUserId]         GUID NULL,
    [StatusUpdatedBy]               tinyint NULL -- 0 Provider 1 Employer - To determine if updated by employer or provider,
    [VacancyReference]              BIGINT NOT NULL,
    [LegacyApplicationId]           GUID NULL
    CONSTRAINT [PK_ApplicationReview] PRIMARY KEY (Id),
    INDEX [IX_ApplicationReview_Ukprn] NONCLUSTERED(Ukprn, Owner),
    INDEX [IX_ApplicationReview_AccountId] NONCLUSTERED(AccountId, Owner),
    INDEX [IX_ApplicationReview_CandidateId] NONCLUSTERED(CandidateId),

	)
 */