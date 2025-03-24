using System.Text.Json;
using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;
using MongoApplication = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.Application;

namespace SFA.DAS.Recruit.Jobs.Features.ApplicationReviewsMigration;

public class LegacyApplicationMapper(ILogger<LegacyApplicationMapper> logger)
{
    public LegacyApplication MapFrom(Guid sourceId, MongoApplication source)
    {
        if (string.IsNullOrWhiteSpace(source!.DisabilityStatus?.ToString()))
        {
            logger.LogWarning("[{ApplicationReviewId}] Failed to find an application disability status value: '{sourceValue}'", sourceId, source?.DisabilityStatus);
        }

        string? qualifications = null;
        if (source is { Qualifications.Count: > 0 })
        {
            qualifications = JsonSerializer.Serialize(source.Qualifications);
        }
        
        return new LegacyApplication
        {
            AddressLine1 = source.AddressLine1,
            AddressLine2 = source.AddressLine2,
            AddressLine3 = source.AddressLine3,
            AddressLine4 = source.AddressLine4,
            ApplicationDate = source.ApplicationDate,
            ApplicationReviewDisabilityStatus = source.DisabilityStatus?.ToString() ?? string.Empty,
            BirthDate = source.BirthDate,
            CandidateId = source.CandidateId,
            EducationFromYear = source.EducationFromYear,
            EducationInstitution = source.EducationInstitution,
            EducationToYear = source.EducationToYear,
            Email = source.Email,
            FirstName = source.FirstName,
            HobbiesAndInterests = source.HobbiesAndInterests,
            Id = sourceId,
            Improvements = source.Improvements,
            LastName = source.LastName,
            Phone = source.Phone,
            Postcode = source.Postcode,
            Qualifications = qualifications,
            Skills = string.Join(',', source.Skills),
            Strengths = source.Strengths,
            Support = source.Support,
        };
    }
}