using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain;
using SFA.DAS.Recruit.Jobs.DataAccess.Sql.Domain;
using MongoApplication = SFA.DAS.Recruit.Jobs.DataAccess.MongoDb.Domain.Application;

namespace SFA.DAS.Recruit.Jobs.Features.ApplicationReviewsMigration;

[ExcludeFromCodeCoverage]
public class LegacyApplicationMapper(ILogger<LegacyApplicationMapper> logger)
{
    private static DateTime? ImportDate(DateTime? value)
    {
        if (value is null)
        {
            return null;
        }

        if (value?.Year < 1900 || value?.Year > DateTime.Now.Year)
        {
            return null;
        }

        return value;
    }

    private static string? StringOrNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
    
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
            AddressLine2 = StringOrNull(source.AddressLine2),
            AddressLine3 = StringOrNull(source.AddressLine3),
            AddressLine4 = StringOrNull(source.AddressLine4),
            ApplicationDate = ImportDate(source.ApplicationDate),
            ApplicationReviewDisabilityStatus = source.DisabilityStatus?.ToString() ?? ApplicationReviewDisabilityStatus.Unknown.ToString(),
            BirthDate = ImportDate(source.BirthDate),
            CandidateId = source.CandidateId,
            EducationFromYear = source.EducationFromYear == 0 ? null : source.EducationFromYear,
            EducationInstitution = StringOrNull(source.EducationInstitution),
            EducationToYear = source.EducationToYear == 0 ? null : source.EducationToYear,
            Email = source.Email,
            FirstName = source.FirstName,
            HobbiesAndInterests = StringOrNull(source.HobbiesAndInterests),
            Id = sourceId,
            Improvements = StringOrNull(source.Improvements),
            LastName = source.LastName,
            Phone = source.Phone,
            Postcode = source.Postcode,
            Qualifications = qualifications,
            Skills = source.Skills == null ? null : StringOrNull(string.Join(",", source.Skills)),
            Strengths = StringOrNull(source.Strengths),
            Support = StringOrNull(source.Support),
        };
    }
}